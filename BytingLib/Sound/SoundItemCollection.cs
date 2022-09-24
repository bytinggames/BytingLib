using Microsoft.Xna.Framework.Audio;
using System;

namespace BytingLib
{
    public class SoundItemCollection : SoundItemAbstract
    {
        static Random rand = new Random();

        private SoundEffect[] soundEffects;

        public SoundEffect[] SoundEffects
        {
            get => soundEffects;
            set
            {
                if (soundEffects != null)
                {
                    for (int i = 0; i < soundEffects.Length; i++)
                    {
                        soundEffects[i]?.Dispose();
                    }
                }
                soundEffects = value;
            }
        }

        public SoundItemCollection(SoundEffect[] soundEffects)
        {
            SoundEffects = soundEffects;
        }

        public override void Dispose()
        {
            for (int i = 0; i < soundEffects.Length; i++)
            {
                soundEffects[i]?.Dispose();
            }
        }

        public SoundEffect GetRandomSoundEffect() => soundEffects[rand.Next(soundEffects.Length)];

        public override bool Play()
        {
            if (SoundMaster.Muted)
                return true;

            return GetRandomSoundEffect().Play(GetOutputVolume(), Pitch, Pan);
        }
        public override bool Play(float relativeVolume, float relativePitch, float relativePan)
        {
            if (SoundMaster.Muted)
                return true;

            return GetRandomSoundEffect().Play(
                GetOutputVolume(Volume + relativeVolume)
                , Math.Min(Math.Max(Pitch + relativePitch, -1f), 1f)
                , Math.Min(Math.Max(Pan + relativePan, -1f), 1f));
        }
        public override bool Play(float volumeMultiplier)
        {
            if (SoundMaster.Muted)
                return true;

            return GetRandomSoundEffect().Play(
                GetOutputVolume(Volume * volumeMultiplier), Pitch, Pan);
        }
    }
}
