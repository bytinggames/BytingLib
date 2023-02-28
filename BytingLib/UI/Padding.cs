namespace BytingLib.UI
{
    public class Padding
    {
        public float Left { get; set; }
        public float Right { get; set; }
        public float Top { get; set; }
        public float Bottom { get; set; }

        public Padding(float padding)
        {
            Left = Right = Top = Bottom = padding;
        }

        public Padding(float paddingX, float paddingY)
        {
            Left = Right = paddingX;
            Top = Bottom = paddingY;
        }

        public Padding(float left, float top, float right, float bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }

        public void RemoveFromRect(Rect rect)
        {
            rect.Width -= Right + Left;
            rect.Height -= Bottom + Top;
            rect.X += Left;
            rect.Y += Top;
        }

        public void AddToRect(Rect rect)
        {
            rect.Width += Right + Left;
            rect.Height += Bottom + Top;
            rect.X -= Left;
            rect.Y -= Top;
        }

        public Vector2 GetSize()
        {
            return new Vector2(Left + Right, Top + Bottom);
        }

        public float Width => Left + Right;
        public float Height => Top + Bottom;
    }
}
