namespace BytingLib.UI
{
    public class Button : Label
    {
        private readonly Action clickAction;
        private bool hover;
        private bool down;
        private bool disabled;
        public bool Disabled
        {
            get => disabled;
            set
            {
                if (value)
                {
                    hover = false;
                    down = false;
                }
                disabled = value;
            }
        }
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
            if (Disabled)
            {
                return;
            }

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
            int frameIndex;
            if (Disabled)
                frameIndex = 3;
            else
            {
                if (down)
                    frameIndex = 2;
                else if (hover)
                    frameIndex = 1;
                else
                    frameIndex = 0;
            }
            style.ButtonAnimation.DrawSliced(spriteBatch, frameIndex, absoluteRect);

            Anchor textAnchor = absoluteRect.GetCenterAnchor();
            if (down)
                textAnchor.pos += TextShiftOnDown;
            style.Font.Value.Draw(spriteBatch, text, textAnchor, style.ButtonFontColor[frameIndex], roundPositionTo: 1);
        }
    }
}
