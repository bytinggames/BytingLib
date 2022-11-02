using Microsoft.Xna.Framework.Audio;
using System;

namespace BytingLib
{
    //public class SoundItemCollection : SoundItemAbstract
    //{
    //    static Random rand = new Random();

    //    private SoundItem[] sfxs;

    //    public SoundItem[] Sfxs
    //    {
    //        get => sfxs;
    //        set
    //        {
    //            if (sfxs != null)
    //            {
    //                for (int i = 0; i < sfxs.Length; i++)
    //                {
    //                    sfxs[i]?.Dispose();
    //                }
    //            }
    //            sfxs = value;
    //        }
    //    }

    //    public SoundItemCollection(ISoundBus bus, SoundItem[] sfxs)
    //        : base(bus)
    //    {
    //        this.sfxs = sfxs;
    //    }

    //    public Ref<SoundEffect> GetRandomSoundEffect() => sfxs[rand.Next(sfxs.Length)];

    //    public override void Play()
    //    {
    //        if (SoundMaster.Muted)
    //            return true;

    //        return GetRandomSoundEffect().Play(GetOutputVolume(), Pitch, Pan);
    //    }
    //    public override void Play(float relativeVolume, float relativePitch, float relativePan)
    //    {
    //        if (SoundMaster.Muted)
    //            return true;

    //        return GetRandomSoundEffect().Play(
    //            GetOutputVolume(Volume + relativeVolume)
    //            , Math.Min(Math.Max(Pitch + relativePitch, -1f), 1f)
    //            , Math.Min(Math.Max(Pan + relativePan, -1f), 1f));
    //    }
    //    public override void Play(float volumeMultiplier)
    //    {
    //        if (SoundMaster.Muted)
    //            return true;

    //        return GetRandomSoundEffect().Play(
    //            GetOutputVolume(Volume * volumeMultiplier), Pitch, Pan);
    //    }
    //}
}
