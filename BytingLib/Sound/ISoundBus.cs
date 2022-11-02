namespace BytingLib
{
    public interface ISoundBus
    {
        public float Volume { get; }
        public float Pitch { get; }
        public float Pan { get; }

        public event Action<float>? OnVolumeChanged;
        public event Action<float>? OnPitchChanged;
        public event Action<float>? OnPanChanged;
    }
}
