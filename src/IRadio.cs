using System;
using System.Threading.Tasks;

namespace plr
{
    internal interface IRadio : IDisposable
    {
        void Play(string uri);
        void Pause();
        void Start();
        void Stop();
        double Volume(int value);
        string Status();
    }
}