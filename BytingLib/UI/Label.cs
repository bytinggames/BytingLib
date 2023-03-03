namespace BytingLib.UI
{
    public class Label : Element
    {
        protected readonly string text;
        private bool setSizeToText;

        public Label(string text)
        {
            this.text = text;
            setSizeToText = true;
        }
        public Label(string text, float width = -1f, float height = -1f)
        {
            this.text = text;
            Width = width;
            Height = height;
        }

        protected virtual Label SetSizeToText(StyleRoot style)
        {
            Vector2 size = style.Font.Value.MeasureString(text) * style.FontScale;
            Width = size.X;
            Height = size.Y;
            return this;
        }

        protected override void UpdateTreeBeginSelf(StyleRoot style)
        {
            if (setSizeToText)
            {
                SetSizeToText(style);
            }
        }

        protected override void DrawSelf(SpriteBatch spriteBatch, StyleRoot style)
        {
            if (style.FontBoldColor != null)
                style.FontBold?.Value.Draw(spriteBatch, text, absoluteRect.GetAnchor(Anchor), style.FontBoldColor, style.FontScale, roundPositionTo: 1);
            if (style.FontColor != null)
                style.Font.Value.Draw(spriteBatch, text, absoluteRect.GetAnchor(Anchor), style.FontColor, style.FontScale, roundPositionTo: 1f);
        }
    }
}
