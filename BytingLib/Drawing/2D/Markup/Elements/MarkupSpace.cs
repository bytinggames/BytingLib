namespace BytingLib.Markup
{
    [MarkupShortcut("space")]
    public class MarkupSpace : ILeaf
    {
        public bool ConfinesToLineSpacing => false;

        public float Width { get; }
        public float Height { get; }

        public MarkupSpace(float width, float height)
        {
            Width = width;
            Height = height;
        }

        public void Draw(MarkupSettings settings) { }

        public Vector2 GetSize(MarkupSettings settings, int start, int end)
        {
            return new Vector2(Width, Height);
        }

        public override string ToString()
        {
            return "\\n";
        }

        public void Dispose()
        {
        }
    }
}
