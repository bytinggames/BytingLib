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
    }
}
