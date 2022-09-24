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
        public SoundEffect SoundEffect { get; set; }
        public float Volume { get; set; }

        public override void Dispose()
        {
            throw new NotImplementedException();
        }

        public void Play(float v1, object value, float v2)
        {
            throw new NotImplementedException();
        }

        public override bool Play()
        {
            throw new NotImplementedException();
        }

        public override bool Play(float relativeVolume, float relativePitch, float relativePan)
        {
            throw new NotImplementedException();
        }

        public override bool Play(float volumeMultiplier)
        {
            throw new NotImplementedException();
        }
    }
}
