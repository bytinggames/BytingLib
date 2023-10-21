namespace BytingLib
{
    public class SoundBus : ISoundBus
    {
        private float innerVolume;
        private float innerPitch = 0f;
        private float innerPan = 0f;

        public float InnerVolume
        {
            get => innerVolume;
            set
            {
                if (value <= 0)
                {
                    value = 0f;
                }

                if (value != innerVolume)
                {
                    innerVolume = value;
                    OnVolumeChanged?.Invoke(innerVolume);
                }
            }
        }


        public float InnerPitch
        {
            get => innerPitch;
            set
            {
                if (value != innerPitch)
                {
                    innerPitch = value;
                    OnPitchChanged?.Invoke(innerPitch);
                }
            }
        }

        public float InnerPan
        {
            get => innerPan;
            set
            {
                if (value != innerPan)
                {
                    innerPan = value;
                    OnPanChanged?.Invoke(innerPan);
                }
            }
        }

        public virtual float Volume => InnerVolume;
        public virtual float Pitch => InnerPitch;
        public virtual float Pan => InnerPan;

        public SoundBus(float volume)
        {
            InnerVolume = volume;
        }

        public event Action<float>? OnVolumeChanged;
        public event Action<float>? OnPitchChanged;
        public event Action<float>? OnPanChanged;


        protected void InvokeOnVolumeChanged()
        {
            OnVolumeChanged?.Invoke(Volume);
        }
        protected void InvokeOnPitchChanged()
        {
            OnPitchChanged?.Invoke(Pitch);
        }
        protected void InvokeOnPanChanged()
        {
            OnPanChanged?.Invoke(Pan);
        }
    }
}
