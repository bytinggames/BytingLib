using System;

namespace BytingLib
{
    // TODO: check if this class is coded beautifully
    public static class SoundMaster
    {
        private static float volume = 0.5f;
        public static float Volume
        {
            get => volume;
            set
            {
                value = Math.Min(1, Math.Max(0, value));
                if (volume != value)
                {
                    volume = value;
                    OnVolumeChanged?.Invoke(volume);
                }
            }
        }
        static float volumeBeforeMute = Volume;

        public static bool Muted
        {
            get => volume <= 0f;
            set
            {
                if (value == Muted)
                    return;

                if (value)
                {
                    volumeBeforeMute = Volume;
                    Volume = 0f;
                }
                else
                    Volume = volumeBeforeMute;
            }
        }

        public static event Action<float> OnVolumeChanged;
    }
}
