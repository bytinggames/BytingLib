namespace BytingLib.UI
{
    public interface ITooltip
    {
        void OnHover(Element hover, string text, bool showInstantlyWhileMoving);
    }

    public class Tooltip : Panel, ITooltip
    {
        private Vector2 mousePos;
        private Action<string> onUpdateTooltipText;
        Element? lastHover;
        Element? newHover;
        string? lastText;
        string? newText;
        bool showInstantlyWhileMoving;
        /// <summary>This variable might have been fast forwarded, if showInstantlyWhileMoving is set</summary>
        float mouseStillForSeconds;
        Vector2? lastOriginPos;
        public float NoMouseMovementToShowInSeconds { get; set; } = 15f / 60f; // 15 frames at 60fps
        public float TooltipOffset { get; set; } = 32f;
        public bool ShowBelowMouseOrHoverElement { get; set; } = false;

        static readonly float MaxMouseMoveSquaredConsideredStill = MathF.Pow(8f, 2f);

        /// <summary>Falls back to this anchor, if the tooltip would be shown on screen this way. If it would leak out of the screen, the anchor will be reversed</summary>
        public Vector2 PreferredAnchor { get; set; } = new Vector2(0.5f, 0f);

        public Tooltip(Action<string> onUpdateTooltipText)
            : base(0f, 0f)
        {
            this.onUpdateTooltipText = onUpdateTooltipText;
            Anchor = PreferredAnchor = new Vector2(0.5f, 0f);
        }

        protected override void UpdateSelf(ElementInput input)
        {
            bool mouseConsideredMoved = mouseStillForSeconds < NoMouseMovementToShowInSeconds && input.Input.MousePosition.Delta.LengthSquared() > MaxMouseMoveSquaredConsideredStill;
            if (mouseConsideredMoved && !showInstantlyWhileMoving
                || newHover == null
                || lastHover != newHover
                || newText == null)
            {
                mouseStillForSeconds = 0;
                lastOriginPos = null;
            }
            else
            {
                mouseStillForSeconds += input.UpdateSpeed.DeltaSecondsF();
            }

            if (showInstantlyWhileMoving && mouseStillForSeconds < NoMouseMovementToShowInSeconds && newHover != null)
            {
                mouseStillForSeconds = NoMouseMovementToShowInSeconds;
            }


            if (newText == null) // if no one hovers right now, reset showInstantlyWhileMoving. Otherwise tooltip won't disappear
            {
                showInstantlyWhileMoving = false;
            }
            else
            {
                if (mouseStillForSeconds >= NoMouseMovementToShowInSeconds)
                {
                    if (newText != lastText)
                    {
                        lastText = newText;
                        onUpdateTooltipText(newText);
                    }
                }
            }
            lastHover = newHover;
            newHover = null;
            newText = null;

            mousePos = input.Input.MousePosition;

            base.UpdateSelf(input);
        }

        protected override void DrawSelf(SpriteBatch spriteBatch, StyleRoot style)
        {
            if (Visible)
            {
                if (NeedsPositionBeUpdated())
                {
                    UpdatePosition();
                    UpdateTreeBegin(style);
                    UpdateTree(AbsoluteRect);
                }
            }

            base.DrawSelf(spriteBatch, style);
        }

        private void UpdatePosition()
        {
            // 1. try to position the tooltip like intended with the PreferredAnchor
            // 2. if outside of screen, reverse anchor and try again
            // 3. if still outside of screen, use PreferredAnchor and push inside screen
            Anchor = PreferredAnchor;
            for (int i = 0; i < 2; i++)
            {
                UpdatePositionInner();

                if (Parent == null)
                {
                    break;
                }
                else
                {
                    if (i == 2)
                    {
                        AbsoluteRect.PushIntoRectangle(Parent.AbsoluteRect);
                        break; // now we are definetely on screen
                    }
                    else if (AbsoluteRect.IsEnclosedIn(Parent.AbsoluteRect))
                    {
                        // we're on screen
                        break;
                    }
                    else
                    {
                        if (i == 0)
                        {
                            // is only overlapping at the right or left of the screen?
                            // then just shift inside of the screen
                            if (AbsoluteRect.Bottom <= Parent.AbsoluteRect.Bottom)
                            {
                                AbsoluteRect.PushIntoRectangle(Parent.AbsoluteRect);
                                break;
                            }
                            Anchor = Vector2.One - PreferredAnchor;
                        }
                        else
                        {
                            AbsoluteRect.PushIntoRectangle(Parent.AbsoluteRect);
                            break;
                        }
                    }
                }
            }

            lastOriginPos = GetOriginPos();
        }

        private bool NeedsPositionBeUpdated()
        {
            return AbsoluteRect != null
                && (lastOriginPos == null || GetOriginPos() != lastOriginPos.Value);
        }

        private void UpdatePositionInner()
        {
            Vector2 originPos = GetOriginPos();
            Vector2 offset = Anchor == new Vector2(0.5f) ? Vector2.Zero
                : TooltipOffset * Vector2.Normalize(new Vector2(0.5f) - Anchor);
            AbsoluteRect.SetPosByOriginNormalized(originPos + offset, Anchor);
        }

        private Vector2 GetOriginPos()
        {
            Vector2 originPos;
            if (ShowBelowMouseOrHoverElement || lastHover == null)
            {
                originPos = mousePos;
            }
            else
            {
                originPos = lastHover.AbsoluteRect.GetPos(Vector2.One - Anchor);
            }

            return originPos;
        }

        public override void Draw(SpriteBatch spriteBatch, StyleRoot style)
        {
            if (mouseStillForSeconds >= NoMouseMovementToShowInSeconds)
            {
                base.Draw(spriteBatch, style);
            }
        }

        public void OnHover(Element hover, string text, bool showInstantlyWhileMoving)
        {
            // make sure the first one that raises the tooltip is not overridden by later tries on raising the tooltip
            if (newHover == null)
            {
                newHover = hover;
                newText = text;
                this.showInstantlyWhileMoving = showInstantlyWhileMoving;
            }
        }

        public bool IsTooltipStartShowingThisUpdate()
        {
            return mouseStillForSeconds == NoMouseMovementToShowInSeconds;
        }

        protected override void UpdateTreeModifyRect(Rect rect)
        {
            lastOriginPos = null;

            base.UpdateTreeModifyRect(rect);
        }
    }
}
