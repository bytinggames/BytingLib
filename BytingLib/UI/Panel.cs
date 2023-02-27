namespace BytingLib.UI
{
    public class Panel : Element
    {
        public Color? Color { get; }

        public Panel(float width, float height, Color? color)
        {
            Width = width;
            Height = height;
            Color = color;
        }
        public Panel(float width, float height, Color? color, Sides? padding, Vector2 anchor)
        {
            Width = width;
            Height = height;
            Padding = padding;
            Anchor = anchor;
            Color = color;
        }

        public override void Draw(SpriteBatch spriteBatch, Rect rect)
        {
            if (Color != null)  
                rect.Draw(spriteBatch, Color.Value);

            for (int i = 0; i < Children.Count; i++)
            {
                var c = Children[i];
                Vector2 size = new Vector2(c.Width >= 0 ? c.Width : -c.Width * rect.Width, c.Height >= 0 ? c.Height : -c.Height * rect.Height);
                Vector2 pos = c.Anchor * rect.Size + rect.Pos;

                Rect myRect = new Anchor(pos, c.Anchor).Rectangle(size);

                Children[i].Draw(spriteBatch, myRect);
            }
        }
    }
}
