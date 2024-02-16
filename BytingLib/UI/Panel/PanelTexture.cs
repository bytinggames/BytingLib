namespace BytingLib.UI
{
    public class PanelTexture : Element
    {
        public Ref<Texture2D> Texture { get; set; }
        public Color Color { get; set; }
        public bool KeepAspectRatio { get; set; } = false;

        public PanelTexture(Ref<Texture2D> texture, float? width = null, float? height = null, Color? color = null, Vector2? anchor = null, Padding? padding = null)
        {
            Width = width ?? texture.Value.Width;
            Height = height ?? texture.Value.Height;
            Texture = texture;
            Color = color ?? Color.White;
            if (anchor != null)
            {
                Anchor = anchor.Value;
            }

            Padding = padding;
        }

        public override void UpdateTree(Rect rect)
        {
            if (KeepAspectRatio)
            {
                rect = rect.CloneRect();
                rect.ShrinkToAspectRatio((float)Texture.Value.Width / Texture.Value.Height, Anchor);
            }

            base.UpdateTree(rect);
        }

        protected override void DrawSelf(SpriteBatch spriteBatch, StyleRoot style)
        {
            Texture.Value.Draw(spriteBatch, AbsoluteRect, Color);
        }

        public PanelTexture SetSizeToTexture()
        {
            Width = Texture.Value.Width;
            Height = Texture.Value.Height;
            return this;
        }

        public PanelTexture ScaleToAspectRatio()
        {
            KeepAspectRatio = true;
            Width = -1f;
            Height = -1f;
            return this;
        }
    }
}
