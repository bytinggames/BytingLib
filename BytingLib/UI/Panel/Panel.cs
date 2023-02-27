namespace BytingLib.UI
{
    public class Panel : Element
    {
        public Color? Color { get; }

        public Panel(float width, float height, Color? color, Vector2? anchor = null, Padding? padding = null)
        {
            Width = width;
            Height = height;
            Color = color;
            if (anchor != null)
                Anchor = anchor.Value;
            Padding = padding;
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            if (Color != null)
                absoluteRect.Draw(spriteBatch, Color.Value);
        }
    }
}
