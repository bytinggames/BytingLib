namespace BytingLib.UI
{
    public class Label : Element
    {
        protected string text;
        private bool setSizeToText;
        private string? textToDraw;
        private string TextToDraw => textToDraw ?? text;

        public Label(string text, float width = 0, float height = 0, bool setSizeToText = true)
        {
            this.text = text;
            Width = width;
            Height = height;
            this.setSizeToText = setSizeToText;
        }

        protected virtual Vector2 MeasureString(StyleRoot style, string text)
        {
            return style.Font.Value.MeasureString(text);
        }

        protected virtual Label SetSizeToText(StyleRoot style)
        {
            if (Width == 0)
            {
                Vector2 size = MeasureString(style, text) * style.FontScale;
                Width = size.X;
                Height = size.Y;
            }
            else
            {
                if (Width < 0)
                    throw new NotImplementedException("this is not implemented yet. It is not trivial, there must be a more complex dependency system in place. Maybe with Funcs that get the width and height values");

                textToDraw = style.Font.Value.WrapText(text, Width, style.FontScale.X);
                Height = MeasureString(style, textToDraw).Y * style.FontScale.Y;
            }
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
                style.FontBold?.Value.Draw(spriteBatch, TextToDraw, absoluteRect.GetAnchor(Anchor), style.FontBoldColor, style.FontScale, roundPositionTo: 1);
            if (style.FontColor != null)
                style.Font.Value.Draw(spriteBatch, TextToDraw, absoluteRect.GetAnchor(Anchor), style.FontColor, style.FontScale, roundPositionTo: 1f);
        }
    }
}
