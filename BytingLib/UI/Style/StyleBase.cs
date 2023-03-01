namespace BytingLib.UI
{
    public class StyleBase : Style
    {
        public StyleBase(Ref<SpriteFont> font, Animation buttonAnimation)
        {
            Font = font;
            ButtonAnimation = buttonAnimation;
            FontScale = Vector2.One;
        }
    }
}
