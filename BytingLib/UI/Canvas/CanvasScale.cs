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
        public Matrix? TransformPre { get; set; }
        public Matrix? TransformPost { get; set; }
        public float MinAspectRatio { get; set; }
        public float MaxAspectRatio { get; set; }
        public CanvasScaling Scaling { get; set; } = CanvasScaling.Default;
        private float scale;
        Rect? lastRenderRect;
        // must only be used for non-replay related stuff
        private readonly IResolution graphicsResolution;
        // define custom msaa, if you want the ui to have a anti-aliasing.
        // If the canvas msaa setting is larger than the backbuffer msaa,
        // the canvas will render to a rendertarget with msaa first before rendering that to the backbuffer
        public int? CustomMsaa { get; set; }
        private RenderTargetBinding? msaaRenderTargetBinding;

        public event Action<SpriteBatch>? DrawBatchBeforeCanvas;

        public CanvasScale(int defaultResX, int defaultResY, Func<Rect> getRenderRect, IResolution graphicsResolution,
            MouseInput mouse, KeyInput keys, GameWindow window, StyleRoot style)
            : base(getRenderRect, mouse, keys, window, style)
        {
            Width = defaultResX;
            Height = defaultResY;
            defaultRect = new Rect(0, 0, defaultResX, defaultResY);
            float aspectRatio = (float)defaultResX / defaultResY;
            MinAspectRatio = aspectRatio;
            MaxAspectRatio = aspectRatio;
            this.graphicsResolution = graphicsResolution;
        }

        protected override ElementInput CreateElementInput(MouseInput mouse, KeyInput keys, GameWindow window)
        {
            MouseTransformed mouseTransformed = new MouseTransformed(mouse.GetState, GetTransform);
            MouseInput mouseNew = new MouseInput(mouseTransformed.GetState, () => mouse.IsActivatedThisFrame);

            return new ElementInput(mouseNew, keys, SetUpdateCatch, window);
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
                    {
                        scaleCanvasRegion = scale / newScale;
                    }

                    scale = newScale;
                }
            }

            Transform =
                Matrix.CreateTranslation(new Vector3(-defaultRect.Size / 2f, 0f).GetRound())
                * Matrix.CreateScale(new Vector3(scale, scale, 1f))
                * Matrix.CreateTranslation(new Vector3(renderRect.Size / 2f, 0f).GetRound());

            if (TransformPre != null)
            {
                Transform = TransformPre.Value * Transform;
            }
            if (TransformPost != null)
            {
                Transform = Transform * TransformPost.Value;
            }
        }

        public override void UpdateTree()
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

            if (Padding != null)
            {
                rect.ApplyPadding(Padding.Left, Padding.Right, Padding.Top, Padding.Bottom);
            }

            Width = rect.Width;
            Height = rect.Height;

            AbsoluteRect = rect.CloneRect().Round();

            StyleRoot.Push(Style);
            for (int i = 0; i < Children.Count; i++)
            {
                Children[i].UpdateTreeBegin(StyleRoot);
            }

            for (int i = 0; i < Children.Count; i++)
            {
                Rect childRect = GetChildRect(rect, Children[i]);
                Children[i].UpdateTree(childRect);
            }

            StyleRoot.Pop(Style);

            base.UpdateTree();
        }
        public override void DrawBatch(SpriteBatch spriteBatch)
        {
            SetDirtyIfResChanged();

            SamplerState samplerState = IsScalingPixelated() 
                && MathF.Abs(0.5f - ((scale + 0.5f) % 1)) < 0.01f // check if scale is roughly a whole number (1, 2, 3, etc.)
                ? SamplerState.PointClamp : SamplerState.LinearClamp;

            Matrix transform = Transform;
            Int2 graphicsRes = graphicsResolution.Resolution;
            if (lastRenderRect != null && graphicsRes.ToVector2() != lastRenderRect.Size)
            {
                // custom transform, so that when watching a replay, it still displays the ui wholly independent of viewers and recorders resolution
                transform *= Matrix.CreateScale(graphicsRes.X / lastRenderRect.Width, graphicsRes.Y / lastRenderRect.Height, 1f);
            }

            UpdateMsaaRenderTargetIfNecessary(spriteBatch.GraphicsDevice);

            BeforeDraw(spriteBatch);
            StyleRoot.SpriteBatchBegin = scissorTest =>
            {
                RasterizerState rs = scissorTest ? rasterizerStateScissor : rasterizerState;

                spriteBatch.Begin(samplerState: samplerState,
                    transformMatrix: transform,
                    rasterizerState: rs);
            };

            RenderTargetBinding[]? rememberBindings = null;
            if (msaaRenderTargetBinding != null)
            {
                rememberBindings = spriteBatch.GraphicsDevice.GetRenderTargets();
                spriteBatch.GraphicsDevice.SetRenderTargets(msaaRenderTargetBinding.Value);
                spriteBatch.GraphicsDevice.Clear(Color.Transparent);
            }
            else
            {
                DrawBatchBeforeCanvas?.Invoke(spriteBatch);
            }

            StyleRoot.SpriteBatchBegin(false);

            StyleRoot.SpriteBatchTransform = transform;
            StyleRoot.Push(Style);
            for (int i = 0; i < Children.Count; i++)
            {
                Children[i].Draw(spriteBatch, StyleRoot);
            }
            StyleRoot.Pop(Style);

            spriteBatch.End();

            if (rememberBindings != null)
            {
                spriteBatch.GraphicsDevice.SetRenderTargets(rememberBindings);

                if (msaaRenderTargetBinding != null)
                {
                    DrawBatchBeforeCanvas?.Invoke(spriteBatch);

                    spriteBatch.Begin();
                    spriteBatch.Draw((RenderTarget2D)msaaRenderTargetBinding.Value.RenderTarget, Vector2.Zero, Color.White);
                    spriteBatch.End();
                }
            }
        }

        private void UpdateMsaaRenderTargetIfNecessary(GraphicsDevice gDevice)
        {
            RenderTarget2D? rt = msaaRenderTargetBinding?.RenderTarget as RenderTarget2D;

            if (CustomMsaa != null && CustomMsaa > gDevice.PresentationParameters.MultiSampleCount)
            {
                var res = graphicsResolution.Resolution;

                if (rt == null
                    || rt.Width != res.X
                    || rt.Height != res.Y
                    || rt.MultiSampleCount != CustomMsaa.Value)
                {
                    // update the msaa render target
                    rt?.Dispose();
                    rt = new RenderTarget2D(gDevice, res.X, res.Y, false, 
                        SurfaceFormat.Color, DepthFormat.None, CustomMsaa.Value, RenderTargetUsage.DiscardContents);
                    msaaRenderTargetBinding = new(rt);
                }
            }
            else if (rt != null)
            {
                rt.Dispose();
                msaaRenderTargetBinding = null;
            }
        }

        protected override void UpdateSelf(ElementInput input)
        {
            SetDirtyIfResChanged();

            if (TreeDirty)
            {
                UpdateTree();
            }

            base.UpdateSelf(input);
        }

        private void SetDirtyIfResChanged()
        {
            Rect newRenderRect = getRenderRect();
            if (!lastRenderRect.EqualValue(newRenderRect))
            {
                SetDirty();
                lastRenderRect = newRenderRect;
            }
        }

        public bool IsScalingPixelated()
        {
            return Scaling == CanvasScaling.PixelArt || Scaling == CanvasScaling.PixelArtResponsiveCanvas;
        }
    }
}
