using BytingLib;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace BytingPipeline
{
    public class AnimationReader : ContentTypeReader<Animation>
    {
        protected override Animation Read(ContentReader input, Animation existingInstance)
        {
            var tex = input.ContentManager.Load<Texture2D>(input.AssetName.Remove(input.AssetName.Length - 3/*"Ani".Length*/));
            return new Animation(tex, input.ReadString());
        }
    }
}