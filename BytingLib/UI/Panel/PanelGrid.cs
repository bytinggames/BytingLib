namespace BytingLib.UI
{
    public partial class PanelGrid : Element
    {
        public Color? Color { get; set; }
        public int Columns { get; }
        public float Gap { get; set; }
        public bool ItemsVerticalDirection { get; set; } = false;
        public bool ItemsStartLeft { get; set; } = true;
        public bool ItemsStartTop { get; set; } = true;

        public PanelGrid(Padding padding, float gap, Vector2 anchor, Color? color, int columns)
        {
            Padding = padding;
            Gap = gap;
            Anchor = anchor;
            Color = color;
            Columns = columns;
        }

        public override void UpdateTree(Rect parentRect)
        {
            Vector2 pos = Anchor * parentRect.Size + parentRect.Pos;

            Vector2 fieldSize = GetMaxChildSize();

            int columnsTaken = Math.Min(Columns, Children.Count);
            int rowsTaken = (int)MathF.Ceiling((float)Children.Count / Columns);
            Vector2 contentSize = new Vector2(
                columnsTaken * fieldSize.X + (columnsTaken - 1) * Gap,
                rowsTaken * fieldSize.Y + (rowsTaken - 1) * Gap);
            Vector2 contentSizePlusPadding = contentSize + GetPaddingSize();

            Rect rect = new Anchor(pos, Anchor).Rectangle(contentSizePlusPadding);

            absoluteRect = rect.CloneRect();

            if (Children.Count == 0)
                return;

            Padding?.RemoveFromRect(rect);

            Vector2 toNextItem = Vector2.Zero;
            if (ItemsVerticalDirection)
                toNextItem.Y = fieldSize.Y + Gap;
            else
                toNextItem.X = fieldSize.X + Gap;

            Vector2 toNextRow = Vector2.Zero;
            if (ItemsVerticalDirection)
                toNextRow.X = fieldSize.X + Gap;
            else
                toNextRow.Y = fieldSize.Y + Gap;

            pos.X = ItemsStartLeft ? rect.Left : rect.Right;
            pos.Y = ItemsStartTop ? rect.Top : rect.Bottom;
            if (!ItemsStartLeft)
            {
                toNextItem.X = -toNextItem.X;
                toNextRow.X = -toNextRow.X;
                pos.X -= fieldSize.X;
            }
            if (!ItemsStartTop)
            {
                toNextItem.Y = -toNextItem.Y;
                toNextRow.Y = -toNextRow.Y;
                pos.Y -= fieldSize.Y;
            }

            Vector2 startPos = pos;

            for (int i = 0; i < Children.Count; i++)
            {
                var c = Children[i];
                float width = c.Width >= 0 ? c.Width : -c.Width * fieldSize.X;
                float height = c.Height >= 0 ? c.Height : -c.Height * fieldSize.Y;
                Vector2 remainingSpace = fieldSize - new Vector2(width, height);
                c.UpdateTree(new Rect(pos + remainingSpace * c.Anchor, new Vector2(width, height)));

                if (i % Columns < Columns - 1)
                {
                    pos += toNextItem;
                }
                else
                {
                    pos += toNextRow;

                    if (ItemsVerticalDirection)
                        pos.Y = startPos.Y;
                    else
                        pos.X = startPos.X;
                }
            }
        }

        private Vector2 GetMaxChildSize()
        {
            Vector2 max = Vector2.Zero;
            bool allWidthNegative = true;
            bool allHeightNegative = true;
            for (int i = 0; i < Children.Count; i++)
            {
                if (Children[i].Width >= max.X)
                {
                    max.X = Children[i].Width;
                    allWidthNegative = false;
                }
                if (Children[i].Height > max.Y)
                {
                    max.Y = Children[i].Height;
                    allHeightNegative = false;
                }
            }

            if (allWidthNegative)
                max.X = (GetInnerWidth() - Gap * (Columns - 1)) / Columns;
            if (allHeightNegative)
            {
                int rowsTaken = Children.Count / Columns;
                max.Y = (GetInnerHeight() - Gap * (rowsTaken - 1)) / rowsTaken;
            }

            return max;
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            if (Color != null)
                absoluteRect.Draw(spriteBatch, Color.Value);
        }
    }
}
