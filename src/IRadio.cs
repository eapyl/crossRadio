using System;
using System.Threading.Tasks;

namespace plr
{
    internal interface IRadio : IDisposable
    {
        Task<bool> Init();
        void Play(string uri);
        void Pause();
        void Start();
        void Stop();
        double VolumeUp();
        double VolumeDown();
        double Volume(int value);
        string Status();
    }
}