using BytingLib;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace BytingPipeline
{
    public class AnimationReader : ContentTypeReader<Animation>
    {
        protected override Animation Read(ContentReader input, Animation existingInstance)
        {
            if (ContentCollector.CurrentContentCollector == null)
                throw new BytingException("CurrentContentCollector is not set");

            var tex = ContentCollector.CurrentContentCollector.Use<Texture2D>(input.AssetName.Remove(input.AssetName.Length - 3/*"Ani".Length*/));
            return new Animation(tex, input.ReadString());
        }
    }
}