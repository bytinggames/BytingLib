using Microsoft.Xna.Framework.Graphics;

namespace BytingLib
{
    public class Animation : IDisposable
    {
        public Ref<Texture2D> Texture { get; }
        public Ref<AnimationData> Data { get; }

        public Animation(Ref<Texture2D> texture, Ref<AnimationData> data)
        {
            this.Texture = texture;
            this.Data = data;
        }

        public void Dispose()
        {
            Texture?.Dispose();
            Data?.Dispose();
        }

        public void Draw(SpriteBatch spriteBatch, string animationTagName, Anchor anchor, double ms)
        {
            Texture.Value.Draw(spriteBatch, anchor, null, Data.Value.GetSourceRectangle(ms, animationTagName));
        }
    }
}
