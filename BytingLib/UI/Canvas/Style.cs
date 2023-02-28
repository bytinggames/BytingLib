namespace BytingLib.UI
{
    public class Style
    {
        public Ref<SpriteFont> Font { get; set; }
        public Animation ButtonAnimation { get; }
        public Color FontColor { get; set; } = Color.Black;

        public Style(Ref<SpriteFont> font, Animation buttonAnimation)
        {
            Font = font;
            ButtonAnimation = buttonAnimation;
        }
    }
}
