namespace BytingLib.UI
{
    public class SimpleCanvasFocus : ICanvasFocus
    {
        public Color Color { get; set; } = Color.White * 0.5f;

        public void Draw(SpriteBatch spriteBatch, Element element)
        {
            element.AbsoluteRect.Draw(spriteBatch, Color);
            element.AbsoluteRect.Outline().ThickenOutside(4f).Draw(spriteBatch, Color.White * 0.75f);
        }
    }
}
