namespace BytingLib.UI
{
    public class PanelAspectRatio : Element
    {
        public float AspectRatio { get; }
        public Color? Color { get; set; }

        public PanelAspectRatio(float aspectRatio = 1f, bool takeFullWidthOrHeight = false, Color? color = null, Vector2? anchor = null, Padding? padding = null)
        {
            Width = takeFullWidthOrHeight ? -1f : 0f;
            Height = takeFullWidthOrHeight ? 0f : -1f;
            AspectRatio = aspectRatio;
            Color = color;
            if (anchor != null)
            {
                Anchor = anchor.Value;
            }

            Padding = padding;
        }

        protected override void UpdateTreeBeginSelf(StyleRoot style)
        {
            base.UpdateTreeBeginSelf(style);
        }

        protected override void UpdateTreeModifyRect(Rect rect)
        {
            base.UpdateTreeModifyRect(rect);
        }

        public override float GetSizeTopToBottom(int d, Vector2 parentContainerSize)
        {
            float parentContainerAspectRatio = parentContainerSize.X / parentContainerSize.Y;
            if (AspectRatio > parentContainerAspectRatio)
            {
                // width is equal to parent width
                if (d == 0)
                {
                    return parentContainerSize.X;
                }
                else
                {
                    return parentContainerSize.X / AspectRatio;
                }
            }
            else
            {
                // height is equal to parent height
                if (d == 0)
                {
                    return parentContainerSize.Y * AspectRatio;
                }
                else
                {
                    return parentContainerSize.Y;
                }
            }
        }

        protected override void DrawSelf(SpriteBatch spriteBatch, StyleRoot style)
        {
            if (Color.HasValue)
            {
                AbsoluteRect.Draw(spriteBatch, Color.Value);
            }
        }
    }
}
