using BytingLib.Markup;

namespace BytingLib.UI
{
    public class LabelMarkup : Label
    {
        MarkupRoot? markup;
        private readonly Creator creator;

        public float MinLineHeight { get; set; }

        public LabelMarkup(string text, Creator creator) : base(text)
        {
            this.creator = creator;
        }
        public LabelMarkup(string text, Creator creator, float width = -1f, float height = -1f)
            : base(text, width, height)
        {
            this.creator = creator;
        }

        protected override Vector2 MeasureString(StyleRoot style, string text)
        {
            using (MarkupRoot tempRoot = new MarkupRoot(creator, text))
                return tempRoot.GetSize(GetDefaultSetting(null!, style));
        }

        protected override string CreateTextToDraw(StyleRoot style)
        {
            return Text; // TODO: implement word wrapping?
        }

        protected override void DrawSelf(SpriteBatch spriteBatch, StyleRoot style)
        {
            if (markup != null)
            {
                if (style.FontBoldColor != null && style.FontBold != null)
                    markup.Draw(new MarkupSettings(spriteBatch, style.FontBold, AbsoluteRect.GetAnchor(Anchor), style.FontBoldColor, Anchor.X, style.FontScale) { RoundPositionTo = 1f });
                if (style.FontColor != null)
                    markup.Draw(GetDefaultSetting(spriteBatch, style));
            }
        }

        private MarkupSettings GetDefaultSetting(SpriteBatch spriteBatch, StyleRoot style) => new MarkupSettings(spriteBatch, style.Font, AbsoluteRect == null ? new Anchor() : AbsoluteRect.GetAnchor(Anchor), style.FontColor, Anchor.X, style.FontScale) { RoundPositionTo = 1f, MinLineHeight = MinLineHeight };

        protected override void DisposeSelf()
        {
            markup?.Dispose();
            markup = null;
        }

        protected override void UpdateTreeBeginSelf(StyleRoot style)
        {
            base.UpdateTreeBeginSelf(style);

            markup?.Dispose();
            markup = new MarkupRoot(creator, TextToDraw);
        }
    }
}
