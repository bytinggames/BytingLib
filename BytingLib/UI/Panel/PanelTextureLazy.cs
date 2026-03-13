namespace BytingLib.UI
{
    public class PanelTextureLazy : Element
    {
        private readonly Func<Ref<Texture2D>> getTexture;

        public Ref<Texture2D>? Texture { get; set; }
        public Color Color { get; set; }
        public bool ReplaceOrMultiplyWithStyleColor { get; set; } = false;
        public PanelTextureLazy(Func<Ref<Texture2D>> getTexture, float width, float height, Color? color = null, Vector2? anchor = null, Padding? padding = null)
        {
            this.getTexture = getTexture;
            Width = width;
            Height = height;
            Color = color ?? Color.White;
            if (anchor != null)
            {
                Anchor = anchor.Value;
            }

            Padding = padding;
        }

        protected override void DrawSelf(SpriteBatch spriteBatch, StyleRoot style)
        {
            if (Texture == null)
            {
                Texture = getTexture();
            }
            Color c = Color;
            if (style.TextureColor != null)
            {
                if (ReplaceOrMultiplyWithStyleColor)
                {
                    c = style.TextureColor.Value;
                }
                else
                {
                    c = new Color(Color.ToVector4() * style.TextureColor.Value.ToVector4());
                }
            }
            Texture.Value.Draw(spriteBatch, AbsoluteRect, c);
        }
    }
}
