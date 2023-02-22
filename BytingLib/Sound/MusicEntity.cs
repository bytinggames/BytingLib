using Microsoft.Xna.Framework.Audio;

namespace BytingLib
{
    /// <summary>Dispose() stops the music and disposes the sound effect instance.</summary>
    public class MusicEntity : IDisposable
    {
        private readonly SoundEffectInstance musicInstance;

        public MusicEntity(Ref<SoundEffect> music, float volume = 0.5f)
        {
            musicInstance = music.Value.CreateInstance();
            musicInstance.IsLooped = true;
            musicInstance.Play();
            musicInstance.Volume = volume;
        }

        public void Dispose()
        {
            musicInstance.Stop();
            musicInstance.Dispose();
        }

        public void TogglePlayStop()
        {
            if (musicInstance.IsDisposed)
                return;

            switch (musicInstance.State)
            {
                case SoundState.Playing:
                    musicInstance.Stop();
                    break;
                case SoundState.Paused:
                case SoundState.Stopped:
                    musicInstance.Play();
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
