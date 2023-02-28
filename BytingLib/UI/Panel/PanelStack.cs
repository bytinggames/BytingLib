﻿namespace BytingLib.UI
{
    public partial class PanelStack : Element
    {
        public Color? Color { get; set; }
        public bool Vertical { get; set; }
        public float Gap { get; set; }

        public PanelStack(bool vertical, Padding padding, float gap, Vector2 anchor, Color? color)
        {
            Vertical = vertical;
            Padding = padding;
            Gap = gap;
            Anchor = anchor;
            Color = color;
        }

        public override void UpdateTree(Rect parentRect)
        {
            Vector2 pos = Anchor * parentRect.Size + parentRect.Pos;
            bool anyUnknownSize;
            Vector2 contentSize = Vertical ? GetContentSizeVertical(out anyUnknownSize) : GetContentSizeHorizontal(out anyUnknownSize);
            Vector2 contentSizePlusPadding = contentSize + GetPaddingSize();

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

        protected override void DrawSelf(SpriteBatch spriteBatch, Style style)
        {
            if (Color != null)
                absoluteRect.Draw(spriteBatch, Color.Value);
        }
    }
}
