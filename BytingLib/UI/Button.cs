namespace BytingLib.UI
{
    public class Button : Element
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
        public Vector2 ChildrenShiftOnDown { get; set; } = Vector2.One;

        public Button(Action clickAction, float width = -1f, float height = -1f, Vector2? anchor = null, Padding? padding = null)
        {
            this.clickAction = clickAction;
            Width = width;
            Height = height;
            if (anchor != null)
                Anchor = anchor.Value;
            Padding = padding;
        }

        public Button AutoPadding(Style style)
        {
            var b = style.ButtonAnimation.Data.Value.meta!.slices![0].keys[0].bounds;
            var f = style.ButtonAnimation.Data.Value.frames!.First().Value.rectangle;
            Padding = new Padding(b.x, b.y, f.Width - b.x - b.w, f.Height - b.y - b.h);

            return this;
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

        protected override void UpdateTreeModifyRect(Rect rect)
        {
            if (down)
                rect.Pos += ChildrenShiftOnDown;
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
        }
    }
}
