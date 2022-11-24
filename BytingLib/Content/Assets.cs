using Microsoft.Xna.Framework.Audio;

namespace BytingLib
{
    /// <summary> Asset Container. Derive from it to support loading assets for a certain section of the game</summary>
    public class Assets : IDisposable
    {
        private readonly List<IDisposable> disposables = new List<IDisposable>();
        public ISoundBus SoundBus { get; }
        private readonly SoundSettings soundSettings;

        public Assets(ISoundBus soundBus, SoundSettings soundSettings)
        {
            this.SoundBus = soundBus;
            this.soundSettings = soundSettings;
        }

        protected SoundItem Load(Func<Ref<SoundEffect>> soundFunc)
        {
            Ref<SoundEffect> soundRef = soundFunc();
            disposables.Add(soundRef);
            SoundItem sound = new SoundItem(SoundBus, soundRef);
            if (soundSettings.Settings.TryGetValue(sound.Sfx.Value.Name, out var setting))
            {
                setting.ApplyTo(sound);
            }
            return sound;
        }

        protected T Load<T>(Func<Ref<T>> loadFunc)
        {
            Ref<T> reference = loadFunc();
            disposables.Add(reference);
            return reference.Value;
        }

        public void Dispose()
        {
            for (int i = disposables.Count - 1; i >= 0; i--)
            {
                disposables[i].Dispose();
            }
            disposables.Clear();
        }
    }
}
