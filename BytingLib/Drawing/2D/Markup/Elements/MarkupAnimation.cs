using BytingLib.Creation;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace BytingLib.Markup
{
    [MarkupShortcut("anim")]
    public class MarkupAnimation : MarkupTexture
    {
        private readonly Ref<AnimationData> animationData;
        private readonly string frameTag;

        // play the whole animation
        public MarkupAnimation(IContentCollector content, string texName) : base(content, texName)
        {
            animationData = content.Use<AnimationData>("Textures/" + texName + ".ani");
            frameTag = null;
        }

        public MarkupAnimation(IContentCollector content, string texName, string animationTagName) : base(content, texName)
        {
            animationData = content.Use<AnimationData>("Textures/" + texName + ".ani");
            frameTag = animationTagName;
        }

        protected override Vector2 GetSizeChildUnscaled(MarkupSettings settings)
        {
            return animationData.Value.GetSourceRectangle(settings.TotalMilliseconds, frameTag).Size.ToVector2();
        }

        protected override void DrawChild(MarkupSettings settings)
        {
            SourceRectangle = animationData.Value.GetSourceRectangle(settings.TotalMilliseconds, frameTag);
            base.DrawChild(settings);
        }

        public override void Dispose()
        {
            animationData.Dispose();

            base.Dispose();
        }
    }
}
