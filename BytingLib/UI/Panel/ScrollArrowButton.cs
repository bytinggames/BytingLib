

namespace BytingLib.UI
{
    class ScrollArrowButton : Button
    {
        private readonly bool arrowUp;

        public ScrollArrowButton(bool arrowUp, Action clickAction, float width = 0, float height = 0, Vector2? anchor = null, Padding? padding = null) 
            : base(clickAction, width, height, anchor, padding)
        {
            this.arrowUp = arrowUp;
        }

        protected override void DrawSelf(SpriteBatch spriteBatch, StyleRoot style)
        {
            base.DrawSelf(spriteBatch, style);

            style.ScrollArrow.Value.Draw(spriteBatch, AbsoluteRect.GetCenterAnchor(), _rotation: arrowUp ? 0f : MathHelper.Pi, _color: style.FontColor);
        }
    }
}
