namespace BytingLib.UI
{
    public class Label : Element
    {
        private string _text;
        public string Text
        {
            get => _text;
            set
            {
                if (_text != value)
                {
                    _text = value;
                    SetDirty();
                }
            }
        }
        private bool setSizeToText;
        private string? textToDraw;
        protected string TextToDraw => textToDraw ?? Text;

        /// <summary>Does not affect positioning. Only affects visual rotation</summary>
        public float Tilt { get; set; } = 0f;

        public Label(string text, float width = 0, float height = 0, bool setSizeToText = true)
        {
            _text = text;
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
                {
                    throw new NotImplementedException("this is not implemented yet. It is not trivial, there must be a more complex dependency system in place. Maybe with Funcs that get the width and height values");
                }

                textToDraw ??= CreateTextToDraw(style);
                Height = MeasureString(style, textToDraw).Y * style.FontScale.Y;
            }
            return this;
        }

        protected override void UpdateTreeBeginSelf(StyleRoot style)
        {
            if (setSizeToText)
            {
                Width = 0f; // trigger setting size to text
            }

            textToDraw = CreateTextToDraw(style);

            if (setSizeToText)
            {
                SetSizeToText(style);
            }
            base.UpdateTreeBeginSelf(style);
        }

        protected string CreateTextToDraw(StyleRoot style)
        {
            return CreateTextToDraw(style, out _);
        }

        protected virtual string CreateTextToDraw(StyleRoot style, out List<(int Index, int Add)>? textLengthChanges)
        {
            if (Width > 0)
            {
                return SpriteFontExtension.WrapText(Text, Width, style.FontScale.X, str => MeasureString(style, str), out textLengthChanges);
            }
            textLengthChanges = null;
            return Text;
        }

        protected override void DrawSelf(SpriteBatch spriteBatch, StyleRoot style)
        {
            if (style.FontBoldColor != null)
            {
                style.FontBold?.Value.Draw(spriteBatch, TextToDraw, AbsoluteRect.GetAnchor(Anchor), style.FontBoldColor, style.FontScale, Tilt, roundPositionTo: style.RoundPositionTo);
            }

            if (style.FontColor != null)
            {
                style.Font.Value.Draw(spriteBatch, TextToDraw, AbsoluteRect.GetAnchor(Anchor), style.FontColor, style.FontScale, Tilt, roundPositionTo: style.RoundPositionTo);
            }
        }
    }
}
