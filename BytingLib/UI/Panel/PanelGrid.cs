namespace BytingLib.UI
{
    public partial class PanelGrid : Element
    {
        public Color? Color { get; set; }
        public int Columns { get; }
        public Vector2 Gap { get; set; }
        public bool ItemsVerticalDirection { get; set; } = false;
        public bool ItemsStartLeft { get; set; } = true;
        public bool ItemsStartTop { get; set; } = true;

        public PanelGrid(Padding? padding, Vector2 gap, Vector2 anchor, Color? color, int columns)
        {
            Padding = padding;
            Gap = gap;
            Anchor = anchor;
            Color = color;
            Columns = columns;
            Width = 0f;
            Height = 0f;
        }

        public override void UpdateTree(Rect parentRect)
        {
            Vector2 pos = Anchor * parentRect.Size + parentRect.Pos;
            Vector2 fieldSize, contentSizePlusPadding;

            GetSize(out fieldSize, out contentSizePlusPadding);

            if (contentSizePlusPadding.X < 0)
            {
                contentSizePlusPadding.X = parentRect.Width * -contentSizePlusPadding.X;

                float innerSize = contentSizePlusPadding.X - Padding.WidthOr0();
                innerSize -= (Columns - 1) * Gap.X;
                fieldSize.X = innerSize / Columns;
            }
            if (contentSizePlusPadding.Y < 0)
            {
                contentSizePlusPadding.Y = parentRect.Height * -contentSizePlusPadding.Y;

                float innerSize = contentSizePlusPadding.Y - Padding.HeightOr0();
                int rows = GetRowsTaken();
                innerSize -= (rows - 1) * Gap.Y;
                fieldSize.Y = innerSize / rows;
            }


            Rect rect = new Anchor(pos, Anchor).Rectangle(contentSizePlusPadding);

            AbsoluteRect = rect.CloneRect().Round();

            if (Children.Count == 0)
            {
                return;
            }

            Padding?.RemoveFromRect(rect);

            Vector2 toNextItem = Vector2.Zero;
            if (ItemsVerticalDirection)
            {
                toNextItem.Y = fieldSize.Y + Gap.Y;
            }
            else
            {
                toNextItem.X = fieldSize.X + Gap.X;
            }

            Vector2 toNextRow = Vector2.Zero;
            if (ItemsVerticalDirection)
            {
                toNextRow.X = fieldSize.X + Gap.X;
            }
            else
            {
                toNextRow.Y = fieldSize.Y + Gap.Y;
            }

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
                Rect r = GetChildRect(new Rect(pos, fieldSize), c);
                c.UpdateTree(r);

                if (i % Columns < Columns - 1)
                {
                    pos += toNextItem;
                }
                else
                {
                    pos += toNextRow;

                    if (ItemsVerticalDirection)
                    {
                        pos.Y = startPos.Y;
                    }
                    else
                    {
                        pos.X = startPos.X;
                    }
                }
            }
        }

        private void GetSize(out Vector2 fieldSize, out Vector2 contentSizePlusPadding)
        {
            fieldSize = GetMaxChildSize();
            int columnsTaken = Math.Min(Columns, Children.Count);
            int rowsTaken = GetRowsTaken();
            Vector2 contentSize = new Vector2(
                columnsTaken * fieldSize.X + (columnsTaken - 1) * Gap.X,
                rowsTaken * fieldSize.Y + (rowsTaken - 1) * Gap.Y);
            contentSizePlusPadding = contentSize + GetPaddingSize();

            if (fieldSize.X < 0)
            {
                contentSizePlusPadding.X = fieldSize.X;
            }

            if (fieldSize.Y < 0)
            {
                contentSizePlusPadding.Y = fieldSize.Y;
            }
        }

        private int GetRowsTaken()
        {
            return (int)MathF.Ceiling((float)Children.Count / Columns);
        }

        private Vector2 GetMaxChildSize()
        {
            float[] max = new float[2];
            bool[] allSizeNegative = new[] { true, true };

            for (int i = 0; i < Children.Count; i++)
            {
                for (int d = 0; d < 2; d++)
                {
                    float size = Children[i].GetSizeTopToBottom(d);
                    if (size >= max[d])
                    {
                        max[d] = size;
                        allSizeNegative[d] = false;
                    }
                }
            }

            if (allSizeNegative[0])
            {
                max[0] = -1f;// (GetWidthTopToBottom() - Gap.X * (Columns - 1)) / Columns;
            }

            if (allSizeNegative[1])
            {
                //int rowsTaken = GetRowsTaken();
                max[1] = -1f;// (GetHeightTopToBottom() - Gap.Y * (rowsTaken - 1)) / rowsTaken;
            }

            return new Vector2(max[0], max[1]);
        }

        public override float GetSizeTopToBottom(int d)
        {
            GetSize(out _, out Vector2 contentSizePlusPadding);
            if (d == 0)
            {
                return contentSizePlusPadding.X;
            }
            else
            {
                return contentSizePlusPadding.Y;
            }
        }

        protected override void DrawSelf(SpriteBatch spriteBatch, StyleRoot style)
        {
            if (Color != null)
            {
                AbsoluteRect.Draw(spriteBatch, Color.Value);
            }
        }
    }
}
