using System;
using Serilog;
using Serilog.Core;
using plr.bss;

namespace plr
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
        private double _volume = 0.2;
        private readonly StationManager _stationManager;

        public Radio(StationManager stationManager)
        {
            _stationManager = stationManager;
        }

        public bool Init() => Bass.Init();

        public void Play(string uri)
        {
            if (_streamId != 0)
            {
                Log.Verbose("Cleaned previous channel");
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

        public void VolumeUp() => Bass.ChannelSetAttribute(_streamId, ChannelAttribute.Volume, _volume < 1 ? _volume += 0.1 : _volume);

        public void VolumeDown() => Bass.ChannelSetAttribute(_streamId, ChannelAttribute.Volume, _volume > 0 ? _volume -= 0.1 : _volume);
    }
}