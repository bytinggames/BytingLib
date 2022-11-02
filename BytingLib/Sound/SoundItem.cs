using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BytingLib
{
    // TODO: check if this class is coded beautifully
    public class SoundItem : SoundItemAbstract
    {
        public Ref<SoundEffect> Sfx { get; }

        public SoundItem(ISoundBus bus, Ref<SoundEffect> sfx)
            : base(bus)
        {
            Sfx = sfx;
        }

        public override void Play()
        {
            Sfx.Value.Play(GetOutputVolume(), GetOutputPitch(), GetOutputPan());
        }

        public override void Play(float volumeMultiplier)
        {
            Sfx.Value.Play(GetOutputVolume(volumeMultiplier), GetOutputPitch(), GetOutputPan());
        }

        public override void Play(float volumeMultiplier, float relativePitch, float relativePan)
        {
            Sfx.Value.Play(GetOutputVolume(volumeMultiplier), GetOutputPitch(relativePitch), GetOutputPan(relativePan));
        }
    }
}
