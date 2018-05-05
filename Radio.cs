using System;
using ManagedBass;
using Serilog.Core;

namespace rsRadio
{
    internal interface IRadio
    {
        bool Init();
        void Play(string uri);
        void Pause();
        void Start();
        void Stop();
        void VolumeUp();
        void VolumeDown();
    }

    internal class Radio: IRadio
    {
        private int _streamId = 0;
        private double _volume = 0.5;
        private readonly Logger _log;
        private readonly StationManager _stationManager;

        public Radio(StationManager stationManager, Logger log)
        {
            _log =  log;
            _stationManager = stationManager;
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
                Bass.ChannelPlay(_streamId);
        }

        public void Pause() => Bass.Pause();

        public void Start() => Bass.Start();

        public void Stop() => Bass.Stop();

        public void VolumeUp() => Bass.ChannelSetAttribute(_streamId, ChannelAttribute.Volume, _volume < 1 ? _volume += 0.1 : _volume);

        public void VolumeDown() => Bass.ChannelSetAttribute(_streamId, ChannelAttribute.Volume, _volume > 0 ? _volume -= 0.1 : _volume);
    }
}