namespace BytingLib.UI
{
    public class Style
    {
        public Ref<SpriteFont> Font { get; set; }
        public Animation ButtonAnimation { get; }

        public Style(Ref<SpriteFont> font, Animation buttonAnimation)
        {
            Font = font;
            ButtonAnimation = buttonAnimation;
        }
    }
}
