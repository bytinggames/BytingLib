namespace BytingLib.UI
{
    public class Label : Element
    {
        protected readonly string text;
        protected readonly Ref<SpriteFont> font;

        public Label(string text, Ref<SpriteFont> font /* TODO: probably should be in a style class */)
        {
            this.text = text;
            this.font = font;
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            font.Value.Draw(spriteBatch, text, absoluteRect.GetCenterAnchor(), Color.Black, roundPositionTo: 1f);
        }
    }
}
