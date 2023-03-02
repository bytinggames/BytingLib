namespace BytingLib.DataTypes
{
    /// <summary>
    /// not a mature class, but can be extended to be used as a Rect alternative that is a ValueType
    /// </summary>
    public struct RectangleF
    {
        public float X, Y, Width, Height;

        public RectangleF(float x, float y, float width, float height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public RectangleF(Vector2 pos, Vector2 size)
        {
            X = pos.X;
            Y = pos.Y;
            Width = size.X;
            Height = size.Y;
        }
    }
}
