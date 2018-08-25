using System;
using Serilog;
using Serilog.Core;
using plr.Providers;
using plr.BassLib;

namespace plr
{
    internal class Radio: IRadio
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

        public void Pause() => Bass.Pause();

        public void Start() => Bass.Start();

        public void Stop() => Bass.Stop();

        public void VolumeUp(double delta = 0.1) => Bass.ChannelSetAttribute(_streamId, ChannelAttribute.Volume, _volume + delta <= 1 ? _volume += delta : _volume);

        public void VolumeDown(double delta = 0.1) => Bass.ChannelSetAttribute(_streamId, ChannelAttribute.Volume, _volume - delta >= 0 ? _volume -= delta : _volume);
    }
}