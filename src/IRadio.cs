namespace plr
{
    internal interface IRadio
    {
        bool Init();
        void Play(string uri);
        void Pause();
        void Start();
        void Stop();
        void VolumeUp(double delta = 0.1);
        void VolumeDown(double delta = 0.1);
    }
}