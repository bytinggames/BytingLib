namespace BytingLib.UI
{
    /// <summary>
    /// Keeps the aspect ratio and scales ui to fit the screen.
    /// </summary>
    public class CanvasScale : Element, IDrawBatch
    {
        private readonly Func<Rect> getRenderRect;

        private Rect defaultRect;
        public int DefaultResX => (int)defaultRect.Width;
        public int DefaultResY => (int)defaultRect.Height;
        public Matrix Transform { get; private set; }
        public float MinAspectRatio { get; set; }
        public float MaxAspectRatio { get; set; }
        public CanvasScaling Scaling { get; set; } = CanvasScaling.Default;
        public Color? ClearColor { get; set; }


        public CanvasScale(int defaultResX, int defaultResY, Func<Rect> getRenderRect)
        {
            Width = defaultResX;
            Height = defaultResY;
            defaultRect = new Rect(0, 0, defaultResX, defaultResY);
            this.getRenderRect = getRenderRect;
            float aspectRatio = (float)defaultResX / defaultResY;
            MinAspectRatio = aspectRatio;
            MaxAspectRatio = aspectRatio;
        }

        protected virtual void SpriteBatchBegin(SpriteBatch spriteBatch, Rect renderRect, out float scaleCanvasRegion)
        {
            Vector2 scale2 = renderRect.Size / defaultRect.Size;
            float scale = MathF.Min(scale2.X, scale2.Y);
            scaleCanvasRegion = 1f;
            if (Scaling == CanvasScaling.PixelArt
                || Scaling == CanvasScaling.PixelArtResponsiveCanvas)
            {
                if (scale > 1f)
                {
                    float newScale = MathF.Floor(scale);
                    if (Scaling == CanvasScaling.PixelArtResponsiveCanvas)
                        scaleCanvasRegion = scale / newScale;
                    scale = newScale;
                }
            }

            Transform =
                Matrix.CreateTranslation(new Vector3(-defaultRect.Size / 2f, 0f))
                * Matrix.CreateScale(new Vector3(scale, scale, 0f))
                * Matrix.CreateTranslation(new Vector3(renderRect.Size / 2f, 0f));

            spriteBatch.Begin(transformMatrix: Transform);
        }

        public void DrawBatch(SpriteBatch spriteBatch)
        {
            if (ClearColor != null)
                spriteBatch.GraphicsDevice.Clear(ClearColor.Value);

            Rect renderRect = getRenderRect();

            SpriteBatchBegin(spriteBatch, renderRect, out float scaleCanvasRegion);

            Rect rect = defaultRect.CloneRect();

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

            if (scaleCanvasRegion != 1f)
            {
                Vector2 newSize = rect.Size * scaleCanvasRegion;
                rect.Pos -= (newSize - rect.Size) / 2f;
                rect.Size = newSize;
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
