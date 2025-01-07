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
        int mouseStillForFrames;
        public int NoMouseMovementToShowInFrames { get; set; } = 15;
        public Vector2 TooltipOffset { get; set; } = new Vector2(0f, 32f);
        public bool ShowBelowMouseOrHoverElement { get; set; } = false;
        bool appearedThisUpdate;

        static readonly float MaxMouseMoveSquaredConsideredStill = MathF.Pow(8f, 2f);

        public Tooltip(Action<string> onUpdateTooltipText)
            : base(0f, 0f)
        {
            this.onUpdateTooltipText = onUpdateTooltipText;
            Anchor = new Vector2(0.5f, 0f);
        }

        protected override void UpdateSelf(ElementInput input)
        {
            appearedThisUpdate = false;

            bool mouseConsideredMoved = mouseStillForFrames < NoMouseMovementToShowInFrames && input.Mouse.Move.LengthSquared() > MaxMouseMoveSquaredConsideredStill;
            if (mouseConsideredMoved && !showInstantlyWhileMoving
                || newHover == null
                || lastHover != newHover
                || newText == null)
            {
                mouseStillForFrames = 0;
            }
            else
            {
                mouseStillForFrames++;
            }

            if (showInstantlyWhileMoving && mouseStillForFrames < NoMouseMovementToShowInFrames)
            {
                mouseStillForFrames = NoMouseMovementToShowInFrames;
            }


            if (newText == null) // if no one hovers right now, reset showInstantlyWhileMoving. Otherwise tooltip won't disappear
            {
                showInstantlyWhileMoving = false;
            }
            else
            {
                if (mouseStillForFrames >= NoMouseMovementToShowInFrames)
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

            mousePos = input.Mouse.Position;

            base.UpdateSelf(input);
        }

        protected override void DrawSelf(SpriteBatch spriteBatch, StyleRoot style)
        {
            Vector2 originPos;
            if (ShowBelowMouseOrHoverElement || lastHover == null)
            {
                originPos = mousePos;
            }
            else
            {
                originPos = lastHover.AbsoluteRect.BottomV;
            }
            AbsoluteRect.SetPosByOriginNormalized(originPos + TooltipOffset, Anchor);
            if (Parent != null)
            {
                AbsoluteRect.PushIntoRectangle(Parent.AbsoluteRect);
            }
            UpdateTreeBegin(style);
            UpdateTree(AbsoluteRect);

            base.DrawSelf(spriteBatch, style);
        }

        public override void Draw(SpriteBatch spriteBatch, StyleRoot style)
        {
            if (mouseStillForFrames >= NoMouseMovementToShowInFrames)
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
            return mouseStillForFrames == NoMouseMovementToShowInFrames;
        }
    }
}
