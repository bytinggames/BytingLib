namespace BytingLib.UI
{
    public class Style
    {

        public Ref<SpriteFont> Font { get; set; }
        public Ref<SpriteFont> FontBold { get; set; }
        public Animation ButtonAnimation { get; }
        public Color? FontColor { get; set; } = Color.Black;
        public Color? FontBoldColor { get; set; } = null;

        public Style(Ref<SpriteFont> font, Ref<SpriteFont> fontBold, Animation buttonAnimation)
        {
            Font = font;
            FontBold = fontBold;
            ButtonAnimation = buttonAnimation;
        }
    }
}
