namespace BytingLib.UI
{
    public class Panel : Element
    {
        public Color? Color { get; set; }

        public Panel(float width = -1f, float height = -1f, Color? color = null, Vector2? anchor = null, Padding? padding = null)
        {
            Width = width;
            Height = height;
            Color = color;
            if (anchor != null)
                Anchor = anchor.Value;
            Padding = padding;
        }

        protected override void DrawSelf(SpriteBatch spriteBatch, StyleRoot style)
        {
            if (Color != null)
                AbsoluteRect.Draw(spriteBatch, Color.Value);
        }
    }
}
