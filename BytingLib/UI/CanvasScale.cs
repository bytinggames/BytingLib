namespace BytingLib.UI
{
    /// <summary>
    /// Keeps the aspect ratio and scales ui to fit the screen.
    /// </summary>
    public class CanvasScale : Element, IDrawBatch
    {
        private readonly Func<Rect> getRenderRect;
        private readonly Color? clearColor;

        private Rect defaultRect;
        public int DefaultResX => (int)defaultRect.Width;
        public int DefaultResY => (int)defaultRect.Height;
        public Matrix Transform { get; private set; }
        public float MinAspectRatio { get; set; }
        public float MaxAspectRatio { get; set; }

        public CanvasScale(int defaultResX, int defaultResY, Func<Rect> getRenderRect, float? minAspectRatio = null, float? maxAspectRatio = null, Color? clearColor = null)
        {
            Width = defaultResX;
            Height = defaultResY;
            defaultRect = new Rect(0, 0, defaultResX, defaultResY);
            this.getRenderRect = getRenderRect;
            float aspectRatio = (float)defaultResX / defaultResY;
            MinAspectRatio = minAspectRatio ?? aspectRatio;
            MaxAspectRatio = maxAspectRatio ?? aspectRatio;
            this.clearColor = clearColor;
        }

        protected virtual void SpriteBatchBegin(SpriteBatch spriteBatch, Rect renderRect)
        {
            Vector2 scale2 = renderRect.Size / defaultRect.Size;
            float scale = MathF.Min(scale2.X, scale2.Y);

            Transform =
                Matrix.CreateTranslation(new Vector3(-defaultRect.Size / 2f, 0f))
                * Matrix.CreateScale(new Vector3(scale, scale, 0f))
                * Matrix.CreateTranslation(new Vector3(renderRect.Size / 2f, 0f));

            spriteBatch.Begin(transformMatrix: Transform);
        }

        public void DrawBatch(SpriteBatch spriteBatch)
        {
            if (clearColor != null)
                spriteBatch.GraphicsDevice.Clear(clearColor.Value);

            Rect renderRect = getRenderRect();

            SpriteBatchBegin(spriteBatch, renderRect);

            Rect rect = defaultRect.CloneRect();
            //Vector2 scale2 = renderRect.Size / defaultRect.Size;

            float renderAspectRatio = renderRect.Width / renderRect.Height;
            renderAspectRatio = Math.Clamp(renderAspectRatio, MinAspectRatio, MaxAspectRatio);

            float aspectRatioScale = renderAspectRatio / ((float)defaultRect.Width / defaultRect.Height);

            if (aspectRatioScale > 1f)
            {
                float newWidth = rect.Width * aspectRatioScale;
                rect.X -= (newWidth - rect.Width) / 2f;
                rect.Width = newWidth;
            }
            else
            {
                float newHeight = rect.Height * (1f / aspectRatioScale);
                rect.Y -= (newHeight - rect.Height) / 2f;
                rect.Height = newHeight;
            }

            Width = rect.Width;
            Height = rect.Height;

            for (int i = 0; i < Children.Count; i++)
            {
                Children[i].Draw(spriteBatch, rect);
            }

            spriteBatch.End();
        }

        public override void Draw(SpriteBatch spriteBatch, Rect parentRect)
        {
            throw new BytingException("Call DrawBatch() instead");
        }
    }
}
