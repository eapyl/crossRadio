namespace plr
{
    internal interface IRadio
    {
        bool Init();
        void Play(string uri);
        void Pause();
        void Start();
        void Stop();
        void VolumeUp(int delta = 10);
        void VolumeDown(int delta = 10);
        string Status();
    }
}