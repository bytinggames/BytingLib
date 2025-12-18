namespace BytingLib.UI
{
    public class Style
    {
        private Ref<SpriteFont>? font;
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

        /// <summary>Setting this resets the StingEdit. Could be improved some time</summary>
        public Ref<SpriteFont>? Font
        {
            get => font;
            set
            {
                font = value;
                // reset string edit, since this is mostly what we want, when setting a font
                StringEdit = IStringEdit.None;
            }
        }
        public FontAndEdit? FontAndEdit
        {
            set
            {
                if (value != null)
                {
                    Font = value.Value.Font;
                    StringEdit = value.Value.Edit;
                }
                else
                {
                    Font = null;
                    StringEdit = null;
                }
            }
        }
    }

    public record struct FontAndEdit
    {
        public Ref<SpriteFont> Font { get; }
        public IStringEdit Edit { get; }

        public FontAndEdit(Ref<SpriteFont> font, IStringEdit edit)
        {
            Font = font;
            Edit = edit;
        }
        public FontAndEdit(Ref<SpriteFont> font)
        {
            Font = font;
            Edit = IStringEdit.None;
        }

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
