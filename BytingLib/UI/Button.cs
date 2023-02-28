namespace BytingLib.UI
{
    public class Button : Label
    {
        private readonly Action clickAction;
        private readonly Animation animation;
        private bool hover;
        private bool down;
        public Vector2 TextShiftOnDown { get; set; } = Vector2.One;

        public Button(string text, Ref<SpriteFont> font /* TODO: probably should be in a style class */, Action clickAction, Animation animation)
            : base(text, font)
        {
            this.clickAction = clickAction;
            this.animation = animation;
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

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            int frameIndex = 0;
            if (down)
                frameIndex = 2;
            else if (hover)
                frameIndex = 1;
            animation.DrawSliced(spriteBatch, frameIndex, absoluteRect);

            Anchor textAnchor = absoluteRect.GetCenterAnchor();
            if (down)
                textAnchor.pos += TextShiftOnDown;
            font.Value.Draw(spriteBatch, text, textAnchor, Color.Black);
        }
    }
}
