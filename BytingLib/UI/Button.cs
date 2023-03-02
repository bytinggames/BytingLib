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
        public bool SetPaddingToButtonStyle { get; set; } = true;

        public Button(Action clickAction, float width = -1f, float height = -1f, Vector2? anchor = null, Padding? padding = null)
        {
            this.clickAction = clickAction;
            Width = width;
            Height = height;
            if (anchor != null)
                Anchor = anchor.Value;
            if (padding != null)
            {
                Padding = padding;
                SetPaddingToButtonStyle = false;
            }
        }

        protected override void UpdateTreeBeginSelf(StyleRoot style)
        {
            if (SetPaddingToButtonStyle)
            {
                AutoPadding(style);
            }
        }

        private void AutoPadding(StyleRoot style)
        {
            Padding = style.ButtonAnimation.GetFacePadding();
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
                    SetDirty();
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
                    SetDirty();

                    input.SetUpdateCatch(null);
                }
            }
        }

        protected override void UpdateTreeModifyRect(Rect rect)
        {
            if (down)
                rect.Pos += ChildrenShiftOnDown;
        }

        protected override void DrawSelf(SpriteBatch spriteBatch, StyleRoot style)
        {
            int frameIndex = GetFrameIndex();
            if (frameIndex >= style.ButtonAnimation.Data.Value.frames?.Count)
                throw new BytingException("button frame does not exist: " + frameIndex + " button animation frames: " + style.ButtonAnimation.Data.Value.frames?.Count);

            style.ButtonAnimation.DrawSliced(spriteBatch, frameIndex, absoluteRect);
        }

        protected override void DrawSelfPost(SpriteBatch spriteBatch, StyleRoot style)
        {
            int frameIndexPost = GetFrameIndex() + 4;

            if (frameIndexPost < style.ButtonAnimation.Data.Value.frames?.Count)
            {
                style.ButtonAnimation.DrawSliced(spriteBatch, frameIndexPost, absoluteRect);
            }
        }

        private int GetFrameIndex()
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

            return frameIndex;
        }

    }
}
