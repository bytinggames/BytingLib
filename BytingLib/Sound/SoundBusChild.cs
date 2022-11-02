namespace BytingLib
{
    public class SoundBusChild : SoundBus, IDisposable
    {
        private readonly SoundBus parentBus;

        public SoundBusChild(SoundBus parentBus, float volume) : base(volume)
        {
            this.parentBus = parentBus;

            parentBus.OnVolumeChanged += InvokeOnVolumeChangedThis;
            parentBus.OnPitchChanged += InvokeOnPitchChangedThis;
            parentBus.OnPanChanged += InvokeOnPanChangedThis;
        }

        public void Dispose()
        {
            parentBus.OnVolumeChanged -= InvokeOnVolumeChangedThis;
            parentBus.OnPitchChanged -= InvokeOnPitchChangedThis;
            parentBus.OnPanChanged -= InvokeOnPanChangedThis;
        }

        private void InvokeOnVolumeChangedThis(float _)
        {
            InvokeOnVolumeChanged();
        }
        private void InvokeOnPitchChangedThis(float _)
        {
            InvokeOnVolumeChanged();
        }
        private void InvokeOnPanChangedThis(float _)
        {
            InvokeOnVolumeChanged();
        }


        public override float Volume => InnerVolume * parentBus.Volume;
        public override float Pitch => InnerPitch + parentBus.Pitch;
        public override float Pan => InnerPan + parentBus.Pan;
    }
}
