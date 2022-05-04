using Microsoft.Xna.Framework.Graphics;

namespace BytingLib
{
    public class Animation
    {
        public Texture2D Texture { get; }
        public AnimationData Data { get; }

        public Animation(Texture2D texture, AnimationData data)
        {
            this.Texture = texture;
            this.Data = data;
        }
    }
}
