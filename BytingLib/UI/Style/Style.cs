namespace BytingLib.UI
{
    public class Style
    {
        public Ref<SpriteFont>? Font { get; set; }
        public Ref<SpriteFont>? FontBold { get; set; }
        public Color? FontColor { get; set; }
        public Color? FontBoldColor { get; set; }
        public Vector2? FontScale { get; set; }
        public Ref<Animation>? ButtonAnimation { get; set; }
        public Padding? ButtonPadding { get; set; } // TODO: make Padding for any element possible and filter with some css style code
        public bool? ButtonPaddingToButtonBorder { get; set; }
        public float? RoundPositionTo { get; set; }
    }
}
