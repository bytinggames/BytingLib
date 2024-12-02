namespace BytingLib.UI
{
    public class PanelTextureFrame : PanelTexture
    {
        private readonly TextureFrame frame;

        public PanelTextureFrame(Ref<Texture2D> texture, int columns, int totalFrames, float? width = null, float? height = null, Color? color = null, Vector2? anchor = null, Padding? padding = null)
            : base(texture, width ?? GetFrameWidth(texture, columns), height ?? GetFrameHeight(texture, columns, totalFrames), color, anchor, padding)
        {
            frame = new(texture, columns, totalFrames);
        }

        private static int GetRows(int totalFrames, int columns)
        {
            return (int)MathF.Ceiling((float)totalFrames / columns);
        }

        private static int GetFrameWidth(Ref<Texture2D> texture, int columns)
        {
            return texture.Value.Width / columns;
        }

        private static int GetFrameHeight(Ref<Texture2D> texture, int columns, int totalFrames)
        {
            return texture.Value.Height / GetRows(totalFrames, columns);
        }

        protected override void DrawSelf(SpriteBatch spriteBatch, StyleRoot style)
        {
            Texture.Value.Draw(spriteBatch, AbsoluteRect, Color, frame.GetRectangle());
        }
    }
}
