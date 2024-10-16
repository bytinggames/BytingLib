﻿namespace BytingLib.UI
{
    public abstract class ButtonParent : Element
    {
        private bool down;
        private bool disabled;
        public bool Disabled
        {
            get => disabled;
            set
            {
                if (value)
                {
                    Hover = false;
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
                    if (Hover)
                    {
                        DoClick();
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
            
            if (Hover && HoverStyle != null)
            {
                style.Push(HoverStyle);
            }
        }

        protected override void PopMyStyle(StyleRoot style)
        {
            base.PopMyStyle(style);

            if (Hover && HoverStyle != null)
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

        protected int GetFrameIndex()
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
            if (Disabled)
            {
                return;
            }

            base.UpdateHoverElement(input);

            Hover = input.HoverElement == this;
        }
    }
}
