using BytingLib.Markup;

namespace BytingLib.UI
{
    public class LabelMarkup : Label
    {
        MarkupRoot markup;

        public LabelMarkup(string text, Creator creator) : base(text)
        {
            markup = new MarkupRoot(creator, text);
        }
        public LabelMarkup(string text, Creator creator, float width = -1f, float height = -1f)
            : base(text, width, height)
        {
            markup = new MarkupRoot(creator, text);
        }

        protected override void DrawSelf(SpriteBatch spriteBatch, StyleRoot style)
        {
            if (style.FontBoldColor != null && style.FontBold != null)
                markup.Draw(new MarkupSettings(spriteBatch, style.FontBold, absoluteRect.GetAnchor(Anchor), style.FontBoldColor, Anchor.X, style.FontScale) { RoundPositionTo = 1f }); 
            if (style.FontColor != null)
                markup.Draw(new MarkupSettings(spriteBatch, style.Font, absoluteRect.GetAnchor(Anchor), style.FontColor, Anchor.X, style.FontScale) { RoundPositionTo = 1f }); 
        }

        protected override void DisposeSelf()
        {
            markup.Dispose();
        }
    }
}
