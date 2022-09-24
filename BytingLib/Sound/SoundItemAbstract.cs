using System;

namespace BytingLib
{
    public abstract class SoundItemAbstract : IDisposable
    {
        private float volume = 0.5f;
        private float pitch = 0f;
        private float pan = 0f;

        public float Volume { get => volume; set => volume = Math.Min(Math.Max(value, 0f), 1f); }
        public float Pitch { get => pitch; set => pitch = Math.Min(Math.Max(value, -1f), 1f); }
        public float Pan { get => pan; set => pan = Math.Min(Math.Max(value, -1f), 1f); }

        public abstract void Dispose();

        public abstract bool Play();
        public abstract bool Play(float relativeVolume, float relativePitch, float relativePan);
        public abstract bool Play(float volumeMultiplier);

        public float GetOutputVolume() => GetOutputVolume(Volume);
        public float GetOutputVolume(float volume) => Math.Min(Math.Max(volume * SoundMaster.Volume, 0f), 1f);

        public void ResetVolumePitchPan()
        {
            volume = 0.5f;
            pitch = 0f;
            pan = 0f;
        }
    }
}
