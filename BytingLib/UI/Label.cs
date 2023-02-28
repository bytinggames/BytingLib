namespace BytingLib.UI
{
    public class Label : Element
    {
        protected readonly string text;

        public Label(string text)
        {
            this.text = text;
        }

        protected override void DrawSelf(SpriteBatch spriteBatch, Style style)
        {
            style.Font.Value.Draw(spriteBatch, text, absoluteRect.GetCenterAnchor(), Color.Black, roundPositionTo: 1f);
        }
    }
}
