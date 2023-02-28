namespace BytingLib.UI
{
    public class Label : Element
    {
        protected readonly string text;

        public Label(string text, Style styleForSettingSizeToText)
        {
            this.text = text;

            SetSizeToText(styleForSettingSizeToText);
        }
        public Label(string text, float width = -1f, float height = -1f)
        {
            this.text = text;
            Width = width;
            Height = height;
        }

        public Label SetSizeToText(Style style)
        {
            Vector2 size = style.Font.Value.MeasureString(text);
            Width = size.X;
            Height = size.Y;
            return this;
        }

        protected override void DrawSelf(SpriteBatch spriteBatch, Style style)
        {
            style.Font.Value.Draw(spriteBatch, text, absoluteRect.GetCenterAnchor(), style.FontColor, roundPositionTo: 1f);
        }
    }
}
