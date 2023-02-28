namespace BytingLib.UI
{
    public class Button : Label
    {
        private readonly Action clickAction;
        private bool hover;
        private bool down;
        public Vector2 TextShiftOnDown { get; set; } = Vector2.One;

        public Button(string text, Action clickAction, float width = -1f, float height = -1f, Vector2? anchor = null)
            : base(text)
        {
            this.clickAction = clickAction;
            Width = width;
            Height = height;
            if (anchor != null)
                Anchor = anchor.Value;
        }

        protected override void UpdateSelf(ElementInput input)
        {
            hover = absoluteRect.CollidesWith(input.Mouse.Position);

            if (hover)
            {
                if (input.Mouse.Left.Pressed)
                {
                    down = true;
                    input.SetUpdateCatch(this);
                }
            }

            if (down)
            {
                if (!input.Mouse.Left.Down)
                {
                    if (hover)
                    {
                        clickAction();
                    }

                    down = false;

                    input.SetUpdateCatch(null);
                }
            }
        }

        protected override void DrawSelf(SpriteBatch spriteBatch, Style style)
        {
            int frameIndex = 0;
            if (down)
                frameIndex = 2;
            else if (hover)
                frameIndex = 1;
            style.ButtonAnimation.DrawSliced(spriteBatch, frameIndex, absoluteRect);

            Anchor textAnchor = absoluteRect.GetCenterAnchor();
            if (down)
                textAnchor.pos += TextShiftOnDown;
            style.Font.Value.Draw(spriteBatch, text, textAnchor, style.ButtonFontColor[frameIndex], roundPositionTo: 1);
        }
    }
}
