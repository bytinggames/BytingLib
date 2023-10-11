namespace BytingLib.UI
{
    public class StyleBase : Style
    {
        public StyleBase(Ref<SpriteFont> font, Ref<Animation> buttonAnimation)
        {
            Font = font;
            ButtonAnimation = buttonAnimation;
            FontScale = Vector2.One;
            FontColor = Color.Black;
            ButtonPaddingToButtonBorder = false;
            RoundPositionTo = 1f;
        }
    }
}
