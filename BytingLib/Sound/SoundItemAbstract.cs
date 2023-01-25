namespace BytingLib
{
    public abstract class SoundItemAbstract
    {
        public ISoundBus Bus { get; }

        public float Volume { get; set; } = 0.5f;
        public float Pitch { get; set; } = 0f;
        public float Pan { get; set; } = 0f;

        public abstract void Play();
        public abstract void Play(float volumeMultiplier);
        public abstract void Play(float volumeMultiplier, float relativePitch, float relativePan);

        public float GetOutputVolume() => Math.Clamp(Volume * Bus.Volume, 0f, 1f);
        protected float GetOutputVolume(float volumeMultiplier) => Math.Clamp(Volume * volumeMultiplier * Bus.Volume, 0f, 1f);

        public float GetOutputPitch() => Math.Clamp(Pitch + Bus.Pitch, -1f, 1f);
        protected float GetOutputPitch(float relativePitch) => Math.Clamp(Pitch + Bus.Pitch + relativePitch, -1f, 1f);

        public float GetOutputPan() => Math.Clamp(Pan + Bus.Pan, -1f, 1f);
        protected float GetOutputPan(float relativePan) => Math.Clamp(Pan + Bus.Pan + relativePan, -1f, 1f);

        public SoundItemAbstract(ISoundBus bus)
        {
            Bus = bus;
        }

        public void ResetVolumePitchPan()
        {
            Volume = 0.5f;
            Pitch = 0f;
            Pan = 0f;
        }
    }
}
