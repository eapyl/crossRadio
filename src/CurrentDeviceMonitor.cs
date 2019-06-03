using System;
using System.Threading;
using ManagedBass;
using Serilog;

namespace plr
{
    internal interface ICurrentDeviceMonitor : IDisposable
    {
        void Start(int channel);
    }

    internal class CurrentDeviceMonitor : ICurrentDeviceMonitor
    {
        private int _channel;
        private string _deviceName = string.Empty;
        private readonly Timer _timer;
        private int i = 0;
        private readonly ILogger _log;
        private readonly Action<string> _output;

        public CurrentDeviceMonitor(ILogger log, Action<string> output)
        {
            _timer = new Timer(_timer_Tick);
            _log = log;
            _output = output;
        }

        public void Start(int channel)
        {
            _channel = channel;
            _timer.Change(0, 1000);
        }

        private (DeviceInfo, int) GetDefaultDevice()
        {
            DeviceInfo inf;
            for (i = 0; Bass.GetDeviceInfo(i, out inf); i++)
            {
                if (inf.IsDefault) 
                {
                    return (inf, i);
                }
            }
            return (new DeviceInfo(), 0);
        }

        void _timer_Tick(Object stateInfo)
        {
            var defaultDevice = GetDefaultDevice();
            if (defaultDevice.Item1.Name != _deviceName && defaultDevice.Item2 > 0)
            {
                _output($"Changed device for playing to {defaultDevice.Item1.Name}");
                Bass.ChannelSetDevice(_channel, defaultDevice.Item2);
                _deviceName = defaultDevice.Item1.Name;
            }
        }

        public void Dispose()
        {
            _timer.Dispose();
        }
    }
}