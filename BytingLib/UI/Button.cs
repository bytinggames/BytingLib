namespace BytingLib.UI
{
    public class Button : Label
    {
        private readonly Action clickAction;
        private bool hover;
        private bool down;

        public Button(string text, Ref<SpriteFont> font /* TODO: probably should be in a style class */, Action clickAction)
            : base(text, font)
        {
            this.clickAction = clickAction;
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
            if (hover)
                absoluteRect.Draw(spriteBatch, Color.White * 0.5f);
            if (down)
                absoluteRect.Draw(spriteBatch, Color.White * 0.5f);

            base.DrawSelf(spriteBatch);
        }
    }
}
