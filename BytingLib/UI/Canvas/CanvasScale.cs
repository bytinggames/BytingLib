namespace BytingLib.UI
{
    /// <summary>
    /// Keeps the aspect ratio and scales ui to fit the screen.
    /// </summary>
    public class CanvasScale : Canvas, IDrawBatch
    {
        private Rect defaultRect;
        public int DefaultResX => (int)defaultRect.Width;
        public int DefaultResY => (int)defaultRect.Height;
        public Matrix Transform { get; private set; }
        public float MinAspectRatio { get; set; }
        public float MaxAspectRatio { get; set; }
        public CanvasScaling Scaling { get; set; } = CanvasScaling.Default;
        private float scale;

        public CanvasScale(int defaultResX, int defaultResY, Func<Rect> getRenderRect, MouseInput mouse)
            : base(getRenderRect, mouse)
        {
            Width = defaultResX;
            Height = defaultResY;
            defaultRect = new Rect(0, 0, defaultResX, defaultResY);
            float aspectRatio = (float)defaultResX / defaultResY;
            MinAspectRatio = aspectRatio;
            MaxAspectRatio = aspectRatio;
        }

        protected override ElementInput CreateElementInput(MouseInput mouse)
        {
            MouseTransformed mouseTransformed = new MouseTransformed(mouse.GetState, GetTransform);
            MouseInput mouseNew = new MouseInput(mouseTransformed.GetState, () => mouse.IsActivatedThisFrame);

            return new ElementInput(mouseNew, SetUpdateCatch);
        }

        private Matrix GetTransform()
        {
            return Transform;
        }

        protected virtual void UpdateTreeBegin(Rect renderRect, out float scaleCanvasRegion)
        {
            Vector2 scale2 = renderRect.Size / defaultRect.Size;
            scale = MathF.Min(scale2.X, scale2.Y);
            scaleCanvasRegion = 1f;
            if (IsScalingPixelated())
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
                Matrix.CreateTranslation(new Vector3(-defaultRect.Size / 2f, 0f).GetRound())
                * Matrix.CreateScale(new Vector3(scale, scale, 1f))
                * Matrix.CreateTranslation(new Vector3(renderRect.Size / 2f, 0f).GetRound());

        }

        public void UpdateTree()
        {
            Rect renderRect = getRenderRect();

            UpdateTreeBegin(renderRect, out float scaleCanvasRegion);

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

            absoluteRect = rect.CloneRect();

            for (int i = 0; i < Children.Count; i++)
            {
                Children[i].UpdateTree(rect);
            }
        }
        public void DrawBatch(SpriteBatch spriteBatch)
        {
            UpdateTree(); // TODO: only update tree when necessary

            if (ClearColor != null)
                spriteBatch.GraphicsDevice.Clear(ClearColor.Value);

            SamplerState samplerState = IsScalingPixelated() 
                && MathF.Abs(0.5f - ((scale + 0.5f) % 1)) < 0.01f // check if scale is roughly a whole number (1, 2, 3, etc.)
                ? SamplerState.PointClamp : SamplerState.LinearClamp;
            spriteBatch.Begin(samplerState: samplerState, transformMatrix: Transform);

            for (int i = 0; i < Children.Count; i++)
            {
                Children[i].Draw(spriteBatch);
            }

            spriteBatch.End();
        }

        protected override void UpdateSelf(ElementInput input)
        {

            UpdateTree(); // TODO: only update tree when necessary
        }

        public bool IsScalingPixelated()
        {
            return Scaling == CanvasScaling.PixelArt || Scaling == CanvasScaling.PixelArtResponsiveCanvas;
        }
    }
}
