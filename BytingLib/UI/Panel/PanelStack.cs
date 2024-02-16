namespace BytingLib.UI
{
    public partial class PanelStack : Element
    {
        public Color? Color { get; set; }
        public bool Vertical { get; set; }
        public float Gap { get; set; }

        public PanelStack(float gap = 0f, bool vertical = true, Padding? padding = null, Vector2? anchor = null, Color? color = null)
        {
            Gap = gap;
            Vertical = vertical;
            Padding = padding;
            if (anchor != null)
            {
                Anchor = anchor.Value;
            }

            Color = color;
            Width = 0f;
            Height = 0f;
        }

        public override void UpdateTree(Rect parentRect)
        {
            Vector2 pos = Anchor * parentRect.Size + parentRect.Pos;
            bool anyUnknownSize;
            Vector2 contentSize, contentSizePlusPadding;
            GetSize(out anyUnknownSize, out contentSize, out contentSizePlusPadding);
            PercentageToPixels(ref contentSizePlusPadding, ref contentSize, parentRect);

            if (Width != 0)
            {
                contentSizePlusPadding.X = parentRect.Width;
                contentSize.X = contentSizePlusPadding.X - Padding.WidthOr0();
            }
            if (Height != 0)
            {
                contentSizePlusPadding.Y = parentRect.Height;
                contentSize.Y = contentSizePlusPadding.Y - Padding.HeightOr0();
            }

            Rect rect = new Anchor(pos, Anchor).Rectangle(contentSizePlusPadding);

            AbsoluteRect = rect.CloneRect().Round();

            if (Children.Count == 0)
            {
                return;
            }

            Padding?.RemoveFromRect(rect);

            pos = rect.TopLeft;

            if (Vertical)
            {
                UpdateTreeVertical(pos, contentSize, anyUnknownSize, rect);
            }
            else
            {
                UpdateTreeHorizontal(pos, contentSize, anyUnknownSize, rect);
            }
        }

        private void PercentageToPixels(ref Vector2 contentSizePlusPadding, ref Vector2 contentSize, Rect parentRect)
        {
            if (contentSizePlusPadding.X < 0)
            {
                contentSizePlusPadding.X = parentRect.Width * -contentSizePlusPadding.X;
                contentSize.X = contentSizePlusPadding.X - Padding.WidthOr0();
            }
            if (contentSizePlusPadding.Y < 0)
            {
                contentSizePlusPadding.Y = parentRect.Height * -contentSizePlusPadding.Y;
                contentSize.Y = contentSizePlusPadding.Y - Padding.HeightOr0();
            }
        }

        private void GetSize(out bool anyUnknownSize, out Vector2 contentSize, out Vector2 contentSizePlusPadding)
        {
            contentSize = Vertical ? GetContentSizeVertical(out anyUnknownSize) : GetContentSizeHorizontal(out anyUnknownSize);
            contentSizePlusPadding = contentSize;
            if (contentSize.X >= 0)
            {
                contentSizePlusPadding.X += Padding.WidthOr0();
            }

            if (contentSize.Y >= 0)
            {
                contentSizePlusPadding.Y += Padding.HeightOr0();
            }
        }

        protected override void DrawSelf(SpriteBatch spriteBatch, StyleRoot style)
        {
            if (Color != null)
            {
                AbsoluteRect.Draw(spriteBatch, Color.Value);
            }
        }

        public override float GetSizeTopToBottom(int d)
        {
            float size = Size(d);
            if (size > 0)
            {
                return size;
            }
            else if (size < 0)
            {
                return size;
            }

            GetSize(out _, out _, out Vector2 contentSizePlusPadding);
            if (d == 0)
            {
                return contentSizePlusPadding.X;
            }
            else
            {
                return contentSizePlusPadding.Y;
            }
        }
    }
}
