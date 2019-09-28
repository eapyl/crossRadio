using System;
using Serilog;
using plr.Providers;
using System.Threading;
using NAudio.Wave;
using System.Net;
using System.IO;
using NAudio.Wave.SampleProviders;
using System.Linq;

namespace plr
{
    internal class Radio : IRadio
    {
        enum StreamingPlaybackState
        {
            Stopped,
            Playing,
            Buffering,
            Paused
        }

        private readonly Timer _timer;
        private readonly ILogger _log = null;
        private volatile StreamingPlaybackState playbackState;
        private volatile bool fullyDownloaded;
        private BufferedWaveProvider bufferedWaveProvider;
        private IWavePlayer waveOut;
        private VolumeWaveProvider16 volumeProvider;
        private HttpWebRequest webRequest;
        private string _icyMeta = string.Empty;
        private string _titleAndArtist = string.Empty;
        private string _uri;

        private MeteringSampleProvider vuMeter = null;
        public double LevelLeftValue = 0;
        public double LevelRightValue = 0;
        private ReadFullyStream readFullyStream;
        private double bufferedSeconds;

        public Radio(
            IConfigurationProvider configurationProvider,
            ILogger log)
        {
            _log = log;
            _timer = new Timer(Timer_Tick);
        }

        private bool IsBufferNearlyFull =>
            bufferedWaveProvider != null &&
                bufferedWaveProvider.BufferLength - bufferedWaveProvider.BufferedBytes
                    < bufferedWaveProvider.WaveFormat.AverageBytesPerSecond / 4;

        private void StreamMp3(object state)
        {
            fullyDownloaded = false;
            var url = (string)state;
            webRequest = (HttpWebRequest)WebRequest.Create(url);
            webRequest.Headers.Clear();
            webRequest.Headers.Add("Icy-MetaData", "1");
            HttpWebResponse resp;
            var metaInt = 0;
            try
            {
                resp = (HttpWebResponse)webRequest.GetResponse();
                if (resp.Headers.AllKeys.Contains("icy-metaint"))
                {
                    metaInt = Convert.ToInt32(resp.GetResponseHeader("icy-metaint"));
                }
            }
            catch (WebException e)
            {
                if (e.Status != WebExceptionStatus.RequestCanceled)
                {
                    _log.Error(e.Message);
                }
                return;
            }
            var buffer = new byte[16384 * 4]; // needs to be big enough to hold a decompressed frame

            IMp3FrameDecompressor decompressor = null;
            try
            {
                using (var responseStream = resp.GetResponseStream())
                {
                    readFullyStream = new ReadFullyStream(responseStream, metaInt);
                    do
                    {
                        if (IsBufferNearlyFull)
                        {
                            _log.Verbose("Buffer getting full, taking a break");
                            Thread.Sleep(500);
                        }
                        else
                        {
                            Mp3Frame frame;
                            try
                            {
                                frame = Mp3Frame.LoadFromStream(readFullyStream);
                            }
                            catch (EndOfStreamException)
                            {
                                fullyDownloaded = true;
                                // reached the end of the MP3 file / stream
                                break;
                            }
                            catch (WebException)
                            {
                                // probably we have aborted download from the GUI thread
                                break;
                            }
                            if (frame == null) break;
                            if (decompressor == null)
                            {
                                // don't think these details matter too much - just help ACM select the right codec
                                // however, the buffered provider doesn't know what sample rate it is working at
                                // until we have a frame
                                decompressor = CreateFrameDecompressor(frame);
                                bufferedWaveProvider = new BufferedWaveProvider(decompressor.OutputFormat);
                                bufferedWaveProvider.BufferDuration =
                                    TimeSpan.FromSeconds(20); // allow us to get well ahead of ourselves
                                //this.bufferedWaveProvider.BufferedDuration = 250;
                            }
                            int decompressed = decompressor.DecompressFrame(frame, buffer, 0);
                            //Debug.WriteLine(String.Format("Decompressed a frame {0}", decompressed));
                            bufferedWaveProvider.AddSamples(buffer, 0, decompressed);
                        }
                    } while (playbackState != StreamingPlaybackState.Stopped);
                    _log.Verbose("Exiting");
                    // was doing this in a finally block, but for some reason
                    // we are hanging on response stream .Dispose so never get there
                    decompressor.Dispose();
                }
            }
            finally
            {
                if (decompressor != null)
                {
                    decompressor.Dispose();
                }
            }
        }

        private static IMp3FrameDecompressor CreateFrameDecompressor(Mp3Frame frame) =>
            new AcmMp3FrameDecompressor(new Mp3WaveFormat(
                frame.SampleRate,
                frame.ChannelMode == ChannelMode.Mono ? 1 : 2,
                frame.FrameLength,
                frame.BitRate));

        public void Play(string uri)
        {
            _log.Verbose($"Start playing {uri}");
            _uri = uri;

            if (playbackState == StreamingPlaybackState.Stopped)
            {
                playbackState = StreamingPlaybackState.Buffering;
                bufferedWaveProvider = null;
                ThreadPool.QueueUserWorkItem(StreamMp3, uri);
                _timer.Change(0, 1000);
            }
            else if (playbackState == StreamingPlaybackState.Paused)
            {
                playbackState = StreamingPlaybackState.Buffering;
            }
        }

        private IWavePlayer CreateWaveOut() => new NAudio.Wave.WaveOutEvent();

        private void OnPlaybackStopped(object sender, StoppedEventArgs e)
        {
            _log.Debug("Playback Stopped");
            if (e.Exception != null)
            {
                _log.Error(string.Format("Playback Error {0}", e.Exception.Message));
            }
        }

        private void Meter_StreamVolume(Object sender, StreamVolumeEventArgs e)
        {
            try
            {
                LevelLeftValue = Math.Truncate(e.MaxSampleValues[0] * 150);
                LevelRightValue = Math.Truncate(e.MaxSampleValues[1] * 150);
            }
            catch
            {
                LevelLeftValue = 0;
                LevelRightValue = 0;
            }
        }

        private void Timer_Tick(Object stateInfo)
        {
            if (playbackState == StreamingPlaybackState.Stopped)
            {
                return;
            }
            if (waveOut == null && bufferedWaveProvider != null)
            {
                _log.Verbose("Creating WaveOut Device");
                waveOut = CreateWaveOut();
                waveOut.PlaybackStopped += OnPlaybackStopped;
                volumeProvider = new VolumeWaveProvider16(bufferedWaveProvider);
                volumeProvider.Volume = 0.05F;
                vuMeter = new MeteringSampleProvider(volumeProvider.ToSampleProvider());
                waveOut.Init(new SampleToWaveProvider(vuMeter));
                vuMeter.StreamVolume += Meter_StreamVolume;
                //waveOut.Init(volumeProvider);
                //progressBarBuffer.Maximum = (int)bufferedWaveProvider.BufferDuration.TotalMilliseconds;
            }
            else if (bufferedWaveProvider != null)
            {
                bufferedSeconds = bufferedWaveProvider.BufferedDuration.TotalSeconds;
                // make it stutter less if we buffer up a decent amount before playing
                if (bufferedSeconds < 0.5 && playbackState == StreamingPlaybackState.Playing && !fullyDownloaded)
                {
                    Pause();
                }
                else if (bufferedSeconds > 4 && playbackState == StreamingPlaybackState.Buffering)
                {
                    Start();
                }
                else if (fullyDownloaded && bufferedSeconds == 0)
                {
                    _log.Verbose("Reached end of stream");
                    Stop();
                }
            }
        }

        public void Start()
        {
            waveOut.Play();
            _log.Debug(String.Format("Started playing, waveOut.PlaybackState={0}", waveOut.PlaybackState));
            playbackState = StreamingPlaybackState.Playing;
        }

        public void Pause()
        {
            playbackState = StreamingPlaybackState.Buffering;
            waveOut.Pause();
            _log.Debug(String.Format("Paused to buffer, waveOut.PlaybackState={0}", waveOut.PlaybackState));
        }

        public string Status() => string.Join(Environment.NewLine, new []
        {
            $"{_uri}",
            $"Volume - {volumeProvider.Volume}",
            readFullyStream.MetadataHeader,
            bufferedSeconds > 0 ? $"Buffered {bufferedSeconds} sec" : string.Empty
        });

        public void Stop()
        {
            if (playbackState == StreamingPlaybackState.Stopped)
            {
                return;
            }
            if (!fullyDownloaded)
            {
                webRequest.Abort();
            }

            playbackState = StreamingPlaybackState.Stopped;
            if (waveOut != null)
            {
                waveOut.Stop();
                waveOut.Dispose();
                waveOut = null;
                vuMeter.StreamVolume -= Meter_StreamVolume;
                vuMeter = null;
            }
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
            LevelLeftValue = 0;
            LevelRightValue = 0;
            // n.b. streaming thread may not yet have exited
            Thread.Sleep(500);
            bufferedSeconds = 0;
        }

        public double Volume(int value) => volumeProvider.Volume = value > 100 ? 1f : (value * 1.0f) / 100;

        public void Dispose()
        {
            _timer?.Dispose();
            readFullyStream?.Dispose();
        }
    }
}