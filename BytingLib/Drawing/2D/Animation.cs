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

        public Animation(Ref<Texture2D> texture, string json)
        {
            this.Texture = texture;
            Data = new Ref<AnimationData>(new Pointer<AnimationData>(AnimationData.FromJson(json)), null);
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
