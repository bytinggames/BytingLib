using System.Diagnostics.CodeAnalysis;

namespace BytingLib.Markup
{
    [MarkupShortcut("slice")]
    public class MarkupTextureSlice : MarkupTexture
    {
        private Ref<Animation> animation;
        private readonly string? sliceName;
        //AnimationData.Meta.Slice slice;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        // play the whole animation
        public MarkupTextureSlice(IContentCollector content, string texName)
            : base(content, texName)
        {
            InitSlice();
        }

        public MarkupTextureSlice(IContentCollector content, string texName, string sliceName)
            : base(content, texName)
        {
            this.sliceName = sliceName;
            InitSlice();
        }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        [MemberNotNull(nameof(animation))]
        protected override void SetTexture(IContentCollector content, string texName)
        {
            animation = content.Use<Animation>("Textures/" + texName + "Ani");
            Texture = animation.Value.TextureRef;

            if (animation.Value.Data.meta == null)
            {
                throw new Exception($"animation.Value.Data.meta of texture {texName} is null");
            }

            if (animation.Value.Data.meta.slices == null)
            {
                throw new Exception($"animation.Value.Data.meta.slices of texture {texName} is null");
            }

            if (animation.Value.Data.meta.slices!.Length == 0)
            {
                throw new Exception($"animation.Value.Data.meta.slices of texture {texName} has a length of 0");
            }

        }

        private void InitSlice()
        {
            AnimationData.Meta.Slice slice;
            if (sliceName == null)
            {
                slice = animation.Value.Data.meta!.slices![0];
            }
            else
            {
                slice = animation.Value.Data.meta!.slices!.FirstOrDefault(f => f.name == sliceName)
                    ?? throw new Exception($"texture {Texture.Value.Name} has no slice named {sliceName}");
            }

            if (slice.keys.Length == 0)
            {
                throw new Exception($"animation.Value.Data.meta.slices[0].keys of texture {Texture.Value.Name} has a length of 0");
            }

            var bounds = slice.keys[0].bounds;
            SourceRectangle = new Rectangle(bounds.x, bounds.y, bounds.w, bounds.h);
        }

        protected override Vector2 GetSizeChildUnscaled(MarkupSettings settings)
        {
            if (SourceRectangle == null)
            {
                return Vector2.One;
            }
            return new Vector2(SourceRectangle!.Value.Width, SourceRectangle!.Value.Height);
        }

        public override void Dispose()
        {
            animation.Dispose();

            base.Dispose();
        }
    }
}
