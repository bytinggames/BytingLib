using System.Diagnostics.CodeAnalysis;

namespace BytingLib.Markup
{
    [MarkupShortcut("anim")]
    public class MarkupAnimation : MarkupTexture
    {
        private Ref<Animation> animation;
        private readonly string? frameTag;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        // play the whole animation
        public MarkupAnimation(IContentCollector content, string texName)
            : base(content, texName)
        {
        }

        public MarkupAnimation(IContentCollector content, string texName, string animationTagName)
            : base(content, texName)
        {
            frameTag = animationTagName;
        }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        [MemberNotNull(nameof(animation))]
        protected override void SetTexture(IContentCollector content, string texName)
        {
            animation = content.Use<Animation>("Textures/" + texName + "Ani");
            Texture = animation.Value.TextureRef;
        }

        protected override Vector2 GetSizeChildUnscaled(MarkupSettings settings)
        {
            return animation.Value.Data.GetSourceRectangle(settings.TotalMilliseconds, frameTag).Size.ToVector2();
        }

        protected override void DrawChild(MarkupSettings settings)
        {
            SourceRectangle = animation.Value.Data.GetSourceRectangle(settings.TotalMilliseconds, frameTag);
            base.DrawChild(settings);
        }

        public override void Dispose()
        {
            animation.Dispose();

            base.Dispose();
        }
    }
}
