namespace BytingLib.UI
{
    public class StyleBase : Style
    {
        public StyleBase(Ref<SpriteFont> font, Ref<Animation> buttonAnimation, Ref<Texture2D> scrollArrow)
        {
            Font = font;
            ButtonAnimation = buttonAnimation;
            ScrollArrow = scrollArrow;
            FontScale = Vector2.One;
            FontColor = Color.Black;
            ButtonPaddingToButtonBorder = false;
            RoundPositionTo = 1f;
            StringEdit = null;
            InsertDashOnWrap = true;
        }
    }
}
