using System;
using Serilog;
using Serilog.Core;
using plr.Providers;
using ManagedBass;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Threading;

namespace plr
{
    internal class Radio : IRadio
    {
        private string _url = string.Empty;
        private int _chan = 0;
        private int deltaVolume = 10;
        private double _volume = 0.2;
        private readonly Timer _timer;
        private int _req = 0;
        private static readonly object Lock = new object();
        private readonly ILogger _log = null;
        private readonly ICurrentDeviceMonitor _deviceMonitor;
        private readonly IConfigurationProvider _configurationProvider = null;
        private string _status = string.Empty;
        private string _icyMeta = string.Empty;
        private string _titleAndArtist = string.Empty;

        public Radio(
            IConfigurationProvider configurationProvider,
            ICurrentDeviceMonitor deviceMonitor,
            ILogger log)
        {
            _log = log;
            _deviceMonitor = deviceMonitor;
            _configurationProvider = configurationProvider;
            _timer = new Timer(_timer_Tick);
        }

        public async Task<bool> Init()
        {
            var configuration = await _configurationProvider.Load();
            _volume =  Convert.ToDouble(configuration.Volume);
            var result = Bass.Init();
            Bass.NetPreBuffer = 0;
            return result;
        }

        public void Play(string uri)
        {
            _log.Verbose($"Start playing {uri}");

            _titleAndArtist = _icyMeta = string.Empty;

            int r;

            lock (Lock) // make sure only 1 thread at a time can do the following
                r = ++_req; // increment the request counter for this request

            _timer.Change(Timeout.Infinite, Timeout.Infinite); // stop prebuffer monitoring

            Bass.StreamFree(_chan); // close old stream

            _status = "Connecting...";

            var c = Bass.CreateStream(uri, 0,
                BassFlags.StreamDownloadBlocks | BassFlags.StreamStatus | BassFlags.AutoFree, StatusProc,
                new IntPtr(r));

            Bass.ChannelSetAttribute(c, ChannelAttribute.Volume, _volume);

            lock (Lock)
            {
                if (r != _req)
                {
                    // there is a newer request, discard this stream
                    if (c != 0)
                        Bass.StreamFree(c);
                    return;
                }

                _chan = c; // this is now the current stream
                // monitor output device changing so we will play always on firth device
                _deviceMonitor.Start(_chan);
                _url = uri;
            }

            if (_chan == 0)
            {
                // failed to open
                _status = "Can't play the stream";
            }
            else _timer.Change(0, 100); // start prebuffer monitoring
        }

        void _timer_Tick(Object stateInfo)
        {
            // percentage of buffer filled
            var progress = Bass.StreamGetFilePosition(_chan, FileStreamPosition.Buffer)
                * 100 / Bass.StreamGetFilePosition(_chan, FileStreamPosition.End);

            if (progress > 75 || Bass.StreamGetFilePosition(_chan, FileStreamPosition.Connected) == 0)
            {
                // over 75% full (or end of download)
                _timer.Change(Timeout.Infinite, Timeout.Infinite); // finished prebuffering, stop monitoring

                _status = "Playing";

                foreach (TagType value in Enum.GetValues(typeof(TagType)))
                {
                    var tagPtr = Bass.ChannelGetTags(_chan, value);
                    if (tagPtr == IntPtr.Zero) continue;

                    foreach (var tag in Extensions.ExtractMultiStringAnsi(tagPtr))
                    {
                        if (tag.StartsWith("icy-name:"))
                            _icyMeta += tag + ";";

                        if (tag.StartsWith("icy-url:"))
                            _icyMeta += tag + ";";

                        _log.Verbose($"{Enum.GetName(typeof(TagType), value)}: {tag}");
                    }
                }

                // get the stream title and set sync for subsequent titles
                GetTitle();

                Bass.ChannelSetSync(_chan, SyncFlags.MetadataReceived, 0, MetaSync); // Shoutcast
                Bass.ChannelSetSync(_chan, SyncFlags.OggChange, 0, MetaSync); // Icecast/OGG

                // set sync for end of stream
                Bass.ChannelSetSync(_chan, SyncFlags.End, 0, EndSync);

                // play it!
                Bass.ChannelPlay(_chan);
            }

            else _status = $"Buffering... {progress}%";
        }

        void EndSync(int Handle, int Channel, int Data, IntPtr User) => _status = "Not Playing";

        void MetaSync(int Handle, int Channel, int Data, IntPtr User) => GetTitle();

        private string FormatString(string value) => string.IsNullOrEmpty(value) ? string.Empty : $"\n{value}";

        public string Status() => $"{_status}; Volume:{_volume * 100}%;{FormatString(_titleAndArtist)}{FormatString(_icyMeta)}";

        void StatusProc(IntPtr buffer, int length, IntPtr user)
        {
            if (buffer != IntPtr.Zero
                && length == 0
                && user.ToInt32() == _req) // got HTTP/ICY tags, and this is still the current request

                _status = Marshal.PtrToStringAnsi(buffer); // display status
        }

        public void Pause() => Bass.Pause();

        public void Start() => Bass.Start();

        public void Stop() => Bass.Stop();

        public double VolumeUp() => UpdateVolume(_volume + AdjustVolumeDelta(deltaVolume));

        public double VolumeDown() => UpdateVolume(_volume - AdjustVolumeDelta(deltaVolume));

        public double Volume(int value) => UpdateVolume(AdjustVolumeDelta(value));

        private double UpdateVolume(double volume)
        {
            if (volume >= 0 && volume <= 1)
            {
                _log.Verbose($"New volume {volume}");
                _volume = volume;
                Bass.ChannelSetAttribute(_chan, ChannelAttribute.Volume, _volume);
            }
            return _volume;
        }

        void GetTitle()
        {
            var meta = Bass.ChannelGetTags(_chan, TagType.META);

            if (meta != IntPtr.Zero)
            {
                // got Shoutcast metadata
                var data = Marshal.PtrToStringUTF8(meta);

                _titleAndArtist = data;
            }
            else
            {
                meta = Bass.ChannelGetTags(_chan, TagType.OGG);

                if (meta == IntPtr.Zero)
                    return;

                // got Icecast/OGG tags
                foreach (var tag in Extensions.ExtractMultiStringUtf8(meta))
                {
                    string artist = null, title = null;

                    if (tag.StartsWith("artist="))
                        artist = tag;

                    if (tag.StartsWith("title="))
                        title = tag;

                    if (title != null)
                        _titleAndArtist = artist != null ? $"{title} - {artist}" : title;
                }
            }
        }

        private double AdjustVolumeDelta(int delta) => delta * 1.0 / 100;
        public void Dispose()
        {
            Bass.Free();
            _timer.Dispose();
        }
    }
}