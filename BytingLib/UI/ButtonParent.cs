namespace BytingLib.UI
{
    public abstract class ButtonParent : Element
    {
        /// <summary>Wether the mouse actually hovers over the button</summary>
        protected bool hover;
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

        private Padding? myPadding;

        public Style? HoverStyle { get; set; }

        public ButtonParent(float width = 0f, float height = 0f, Vector2? anchor = null, Padding? padding = null)
        {
            Width = width;
            Height = height;
            if (anchor != null)
            {
                Anchor = anchor.Value;
            }

            myPadding = padding;

            OnWhileHover += WhileHover;
        }

        protected override void UpdateTreeBeginSelf(StyleRoot style)
        {
            if (myPadding == null)
            {
                if (style.ButtonPaddingToButtonBorder)
                {
                    Padding = style.ButtonAnimation.Value.GetFacePadding();
                }
                else if (style.ButtonPadding != null)
                {
                    Padding = style.ButtonPadding;
                }
            }
            else
            {
                Padding = myPadding;
            }

            base.UpdateTreeBeginSelf(style);
        }

        protected override void UpdateSelf(ElementInput input)
        {
            if (Disabled)
            {
                return;
            }

            // updates hover
            base.UpdateSelf(input);

            if (down)
            {
                if (!input.Mouse.Left.Down)
                {
                    if (hover)
                    {
                        OnClick();
                    }

                    down = false;
                    SetDirty();

                    input.SetUpdateCatch(null);
                }
            }
        }

        protected virtual bool WhileHover(Element _, ElementInput input)
        {
            if (input.Mouse.Left.Pressed)
            {
                down = true;
                input.SetUpdateCatch(this);
                SetDirty();
            }
            return true;
        }

        protected override void UpdateTreeModifyRect(Rect rect)
        {
            if (down)
            {
                rect.Pos += ChildrenShiftOnDown;
            }
        }

        protected override void PushMyStyle(StyleRoot style)
        {
            base.PushMyStyle(style);
            
            if (hover && HoverStyle != null)
            {
                style.Push(HoverStyle);
            }
        }

        protected override void PopMyStyle(StyleRoot style)
        {
            base.PopMyStyle(style);

            if (hover && HoverStyle != null)
            {
                style.Pop(HoverStyle);
            }
        }


        protected override void DrawSelf(SpriteBatch spriteBatch, StyleRoot style)
        {
            int frameIndex = GetFrameIndex();
            if (frameIndex >= style.ButtonAnimation.Value.Data.frames?.Count)
            {
                throw new BytingException("button frame does not exist: " + frameIndex + " button animation frames: " + style.ButtonAnimation.Value.Data.frames?.Count);
            }

            style.ButtonAnimation.Value.DrawSliced(spriteBatch, frameIndex, AbsoluteRect);
        }

        protected override void DrawSelfPost(SpriteBatch spriteBatch, StyleRoot style)
        {
            int frameIndexPost = GetFrameIndex() + 4;

            if (frameIndexPost < style.ButtonAnimation.Value.Data.frames?.Count)
            {
                style.ButtonAnimation.Value.DrawSliced(spriteBatch, frameIndexPost, AbsoluteRect);
            }
        }

        private int GetFrameIndex()
        {
            int frameIndex;
            if (Disabled)
            {
                frameIndex = 3;
            }
            else
            {
                if (down)
                {
                    frameIndex = 2;
                }
                else if (hover)
                {
                    frameIndex = 1;
                }
                else
                {
                    frameIndex = 0;
                }
            }

            return frameIndex;
        }

        public override void LooseFocus()
        {
            hover = false;
            down = false;

            base.LooseFocus();
        }

        protected abstract void OnClick();

        protected override void UpdateHoverElement(ElementInput input)
        {
            if (Disabled)
            {
                return;
            }

            base.UpdateHoverElement(input);

            hover = input.HoverElement == this;
        }
    }
}
