using System;
using Serilog;
using Serilog.Core;
using plr.Providers;
using ManagedBass;

namespace plr
{
    internal class Radio : IRadio
    {
        private int _streamId = 0;
        private double _volume = 0.2;
        private readonly ILogger _log;

        public Radio(ILogger log)
        {
            _log = log;
        }

        public bool Init() => Bass.Init();

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

        public void VolumeUp(int delta = 10) =>
            Bass.ChannelSetAttribute(_streamId, ChannelAttribute.Volume,
                _volume + AdjustVolumeDelta(delta) <= 1 ? _volume += AdjustVolumeDelta(delta) : _volume);

        public void VolumeDown(int delta = 10) =>
            Bass.ChannelSetAttribute(_streamId, ChannelAttribute.Volume,
                _volume - AdjustVolumeDelta(delta) >= 0 ? _volume -= AdjustVolumeDelta(delta) : _volume);

        private double AdjustVolumeDelta(int delta) => delta / 100;
    }
}