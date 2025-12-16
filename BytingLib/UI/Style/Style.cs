namespace BytingLib.UI
{
    public class Style
    {
        public Ref<SpriteFont>? Font { get; set; }
        public Ref<SpriteFont>? FontBold { get; set; }
        public Color? FontColor { get; set; }
        public Color? FontBoldColor { get; set; }
        public Color? TextureColor { get; set; }
        public Vector2? FontScale { get; set; }
        public Vector2? MarkupTextureScale { get; set; }
        public Ref<Animation>? ButtonAnimation { get; set; }
        public Ref<Texture2D>? ScrollArrow { get; set; }
        public Padding? ButtonPadding { get; set; } // TODO: make Padding for any element possible and filter with some css style code
        public bool? ButtonPaddingToButtonBorder { get; set; }
        public float? RoundPositionTo { get; set; }
        public IStringEdit? StringEdit { get; set; }

        public FontAndEdit FontAndEdit
        {
            set
            {
                Font = value.Font;
                StringEdit = value.Edit;
            }
        }
    }

    public record struct FontAndEdit(Ref<SpriteFont> Font, IStringEdit Edit)
    {
        public static implicit operator (Ref<SpriteFont> Font, IStringEdit Edit)(FontAndEdit value)
        {
            return (value.Font, value.Edit);
        }

        public static implicit operator FontAndEdit((Ref<SpriteFont> Font, IStringEdit Edit) value)
        {
            return new FontAndEdit(value.Font, value.Edit);
        }
    }
}
