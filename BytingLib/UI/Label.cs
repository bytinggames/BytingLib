namespace BytingLib.UI
{
    public class Label : Element
    {
        public string Text { get; set; }
        private bool setSizeToText;
        private string? textToDraw;
        protected string TextToDraw => textToDraw ?? Text;

        public Label(string text, float width = 0, float height = 0, bool setSizeToText = true)
        {
            this.Text = text;
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
                Vector2 size = MeasureString(style, Text) * style.FontScale;
                Width = size.X;
                Height = size.Y;
            }
            else
            {
                if (Width < 0)
                    throw new NotImplementedException("this is not implemented yet. It is not trivial, there must be a more complex dependency system in place. Maybe with Funcs that get the width and height values");

                textToDraw ??= CreateTextToDraw(style);
                Height = MeasureString(style, textToDraw).Y * style.FontScale.Y;
            }
            return this;
        }

        protected override void UpdateTreeBeginSelf(StyleRoot style)
        {
            if (Width > 0)
                textToDraw = CreateTextToDraw(style);

            if (setSizeToText)
            {
                SetSizeToText(style);
            }
        }

        protected virtual string CreateTextToDraw(StyleRoot style)
        {
            return SpriteFontExtension.WrapText(Text, Width, style.FontScale.X, str => MeasureString(style, str));
        }

        protected override void DrawSelf(SpriteBatch spriteBatch, StyleRoot style)
        {
            if (style.FontBoldColor != null)
                style.FontBold?.Value.Draw(spriteBatch, TextToDraw, AbsoluteRect.GetAnchor(Anchor), style.FontBoldColor, style.FontScale, roundPositionTo: 1);
            if (style.FontColor != null)
                style.Font.Value.Draw(spriteBatch, TextToDraw, AbsoluteRect.GetAnchor(Anchor), style.FontColor, style.FontScale, roundPositionTo: 1f);
        }
    }
}
