namespace BytingLib.UI
{
    public abstract class ButtonParent : Element, IEnabled
    {
        private bool down;
        private bool enabled = true;
        public bool Enabled
        {
            get => enabled;
            set
            {
                if (!value)
                {
                    Hover = false;
                    down = false;
                }
                enabled = value;
            }
        }
        public Vector2 ChildrenShiftOnDown { get; set; } = Vector2.One;

        private Padding? myPadding;

        public Style? HoverStyle { get; set; }
        public Style? DisabledStyle { get; set; }
        protected override bool CanBeNavigatedOverride => Enabled;
        public event Action<ElementInput>? OnHoldBegin;
        public event Action<ElementInput>? OnHoldSustain;
        /// <summary>Used when you have a button inside a button and you don't want the inner button to get highlighted when you hover on the outer button.</summary>
        public bool ClearButtonStyleWithDefault { get; set; } = false;

        public ButtonParent(float width = 0f, float height = 0f, Vector2? anchor = null, Padding? padding = null)
        {
            Width = width;
            Height = height;
            if (anchor != null)
            {
                Anchor = anchor.Value;
            }

            myPadding = padding;

            OnHoverSustain += WhileHover;
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
            if (!Enabled)
            {
                return;
            }

            // updates hover
            base.UpdateSelf(input);

            if (down)
            {
                if (input.Input.Click.Down)
                {
                    OnHoldSustain?.Invoke(input);
                }
                else
                {
                    if (Hover)
                    {
                        DoClick();
                    }

                    down = false;
                    SetDirty();

                    input.UnsetUpdateCatch(this);
                }
            }
        }

        protected virtual bool WhileHover(Element _, ElementInput input)
        {
            if (input.Input.Click.Pressed)
            {
                down = true;
                input.SetUpdateCatch(this);
                SetDirty();

                OnHoldBegin?.Invoke(input);
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

            style.Push(GetActiveStyle());
        }

        protected override void PopMyStyle(StyleRoot style)
        {
            base.PopMyStyle(style);

            style.Pop(GetActiveStyle());
        }

        private Style? GetActiveStyle()
        {
            if (!Enabled)
            {
                if (DisabledStyle != null)
                {
                    return DisabledStyle;
                }
            }
            else if (Hover)
            {
                if (HoverStyle != null)
                {
                    return HoverStyle;
                }
            }
            else
            {
                if (ClearButtonStyleWithDefault
                    && Style != null)
                {
                    return Style;
                }
            }
            return null;
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

        protected int GetFrameIndex()
        {
            int frameIndex;
            if (!Enabled)
            {
                frameIndex = 3;
            }
            else
            {
                if (down)
                {
                    frameIndex = 2;
                }
                else if (Hover)
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
            Hover = false;
            down = false;

            base.LooseFocus();
        }

        protected abstract void DoClick();

        protected override void UpdateHoverElement(ElementInput input)
        {
            if (!Enabled)
            {
                return;
            }

            base.UpdateHoverElement(input);

            Hover = input.HoverElement == this || input.NavigateElement == this;
        }

        public override void ActivateFromNavigation()
        {
            DoClick();
        }
    }
}
