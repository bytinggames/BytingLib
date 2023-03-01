namespace BytingLib.UI
{
    public partial class PanelStack : Element
    {
        public Color? Color { get; set; }
        public bool Vertical { get; set; }
        public float Gap { get; set; }

        public PanelStack(bool vertical, Padding? padding, float gap, Vector2? anchor = null, Color? color = null)
        {
            Vertical = vertical;
            Padding = padding;
            Gap = gap;
            if (anchor != null)
                Anchor = anchor.Value;
            Color = color;
        }

        public override void UpdateTree(Rect parentRect)
        {
            Vector2 pos = Anchor * parentRect.Size + parentRect.Pos;
            bool anyUnknownSize;
            Vector2 contentSize, contentSizePlusPadding;
            GetSize(out anyUnknownSize, out contentSize, out contentSizePlusPadding);

            Rect rect = new Anchor(pos, Anchor).Rectangle(contentSizePlusPadding);
            absoluteRect = rect.CloneRect().Round();

            if (Children.Count == 0)
                return;

            Padding?.RemoveFromRect(rect);

            pos = rect.TopLeft;

            if (Vertical)
                UpdateTreeVertical(pos, contentSize, anyUnknownSize, rect);
            else
                UpdateTreeHorizontal(pos, contentSize, anyUnknownSize, rect);
        }

        private void GetSize(out bool anyUnknownSize, out Vector2 contentSize, out Vector2 contentSizePlusPadding)
        {
            contentSize = Vertical ? GetContentSizeVertical(out anyUnknownSize) : GetContentSizeHorizontal(out anyUnknownSize);
            contentSizePlusPadding = contentSize + GetPaddingSize();
        }

        protected override void DrawSelf(SpriteBatch spriteBatch, StyleRoot style)
        {
            if (Color != null)
                absoluteRect.Draw(spriteBatch, Color.Value);
        }

        public override float GetHeightTopToBottom()
        {
            GetSize(out _, out _, out Vector2 contentSizePlusPadding);
            return contentSizePlusPadding.Y;
        }
        public override float GetWidthTopToBottom()
        {
            GetSize(out _, out _, out Vector2 contentSizePlusPadding);
            return contentSizePlusPadding.X;
        }
    }
}
