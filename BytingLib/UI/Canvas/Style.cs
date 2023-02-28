namespace BytingLib.UI
{
    public class Style
    {
        public Ref<SpriteFont> Font { get; set; }
        public Animation ButtonAnimation { get; }
        public Color FontColor { get; set; } = Color.Black;
        public Color[] ButtonFontColor { get; set; } = new Color[] { Color.Black, Color.Gray, Color.Black, Color.Black };

        public Style(Ref<SpriteFont> font, Animation buttonAnimation)
        {
            Font = font;
            ButtonAnimation = buttonAnimation;
        }
    }
}
