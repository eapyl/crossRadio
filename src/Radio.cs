using System;
using Serilog;
using Serilog.Core;
using plr.Providers;
using ManagedBass;
using System.Threading.Tasks;

namespace plr
{
    internal class Radio : IRadio
    {
        private int _streamId = 0;
        private int deltaVolume = 10;
        private double _volume = 0.2;
        private readonly ILogger _log;
        private readonly IConfigurationProvider _configurationProvider;

        public Radio(
            IConfigurationProvider configurationProvider,
            ILogger log)
        {
            _log = log;
            _configurationProvider = configurationProvider;
        }

        public async Task<bool> Init()
        {
            var configuration = await _configurationProvider.Load();
            _volume =  Convert.ToDouble(configuration.Volume);
            return Bass.Init();
        }

        public void Play(string uri)
        {
            if (_streamId != 0)
            {
                _log.Verbose("Cleaned previous channel");
                Bass.ChannelStop(_streamId);
            }
            _streamId = Bass.CreateStream(uri, 0,
                BassFlags.StreamDownloadBlocks | BassFlags.StreamStatus | BassFlags.AutoFree, null);

            if (_streamId != 0)
            {
                Bass.ChannelSetAttribute(_streamId, ChannelAttribute.Volume, _volume);
                Bass.ChannelPlay(_streamId);
            }
        }

        public string Status()
        {
            return $"Status - {GetPlayStatus()}; Volume - {_volume * 100}%";

            string GetPlayStatus()
            {
                switch (Bass.ChannelIsActive(_streamId))
                {
                    case PlaybackState.Paused:
                        return nameof(PlaybackState.Paused);
                    case PlaybackState.Playing:
                        return nameof(PlaybackState.Playing);
                    case PlaybackState.Stalled:
                        return nameof(PlaybackState.Stalled);
                    case PlaybackState.Stopped:
                        return nameof(PlaybackState.Stopped);
                }
                return "Unknown";
            }
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
                Bass.ChannelSetAttribute(_streamId, ChannelAttribute.Volume, _volume);
            }
            return _volume;
        }

        private double AdjustVolumeDelta(int delta) => delta * 1.0 / 100;
    }
}