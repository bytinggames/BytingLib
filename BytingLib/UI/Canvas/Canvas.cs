namespace BytingLib.UI
{
    public abstract class Canvas : Element, IUpdate, IDrawBatch
    {
        private Element? updateCatch;
        public Color? ClearColor { get; set; }
        protected readonly Func<Rect> getRenderRect;
        public ElementInput Input { get; }
        public StyleRoot StyleRoot { get; set; }
        private bool treeDirty = true;
        // must only be used for non-replay related stuff
        protected readonly IResolution resolution;

        //private bool scissorTest;
        protected readonly RasterizerState rasterizerState = CreateDefaultRasterizerState();
        protected readonly RasterizerState rasterizerStateScissor;

        /// <summary>
        /// define custom msaa, if you want the ui to have a anti-aliasing.
        /// If the canvas msaa setting is larger than the backbuffer msaa,
        /// the canvas will render to a rendertarget with msaa first before rendering that to the backbuffer
        /// </summary>
        public int? CustomMsaa { get; set; }
        protected RenderTargetBinding? msaaRenderTargetBinding;
        public event Action<SpriteBatch>? DrawBatchBeforeCanvas;

        public Canvas(Func<Rect> getRenderRect, MouseInput mouse, KeyInput keys, GameWindow window, StyleRoot style, IResolution resolution)
        {
            this.getRenderRect = getRenderRect;
            StyleRoot = style;
            this.resolution = resolution;
            Input = CreateElementInput(mouse, keys, window);

            rasterizerStateScissor = CreateDefaultRasterizerState();
            rasterizerStateScissor.ScissorTestEnable = true;
        }

        private static RasterizerState CreateDefaultRasterizerState()
        {
            return new RasterizerState()
            {
                CullMode = CullMode.None
            };
        }

        protected bool TreeDirty => treeDirty;

        protected virtual ElementInput CreateElementInput(MouseInput mouse, KeyInput keys, GameWindow window)
        {
            return new ElementInput(mouse, keys, SetUpdateCatch, window);
        }

        public void Update()
        {
            Input.Mouse.Update();
            Input.HoverElement = null; // reset hover element

            if (updateCatch != null)
            {
                updateCatch.Update(Input);
            }
            else
            {
                UpdateSelf(Input);

                for (int i = 0; i < Children.Count; i++)
                {
                    Children[i].Update(Input);
                }
            }
        }

        public override void Update(ElementInput input)
        {
            throw new BytingException("Call Update()");
        }

        public void SetUpdateCatch(Element? element)
        {
            updateCatch = element;
        }

        public abstract void DrawBatch(SpriteBatch spriteBatch);

        protected override void DrawSelf(SpriteBatch spriteBatch, StyleRoot style)
        {
            throw new BytingException("Call DrawBatch() instead");
        }

        public virtual void UpdateTree()
        {
            treeDirty = false;
        }

        protected void BeforeDraw(SpriteBatch spriteBatch)
        {
            if (treeDirty)
            {
                UpdateTree();
            }

            if (ClearColor != null)
            {
                spriteBatch.GraphicsDevice.Clear(ClearColor.Value);
            }
        }

        public override void SetDirty()
        {
            treeDirty = true;

            base.SetDirty();
        }

        protected void UpdateMsaaRenderTargetIfNecessary(GraphicsDevice gDevice)
        {
            RenderTarget2D? rt = msaaRenderTargetBinding?.RenderTarget as RenderTarget2D;

            if (CustomMsaa != null && CustomMsaa > gDevice.PresentationParameters.MultiSampleCount)
            {
                var res = resolution.Resolution;

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

        protected RenderTargetBinding[]? CustomMsaaBegin(SpriteBatch spriteBatch)
        {
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

            return rememberBindings;
        }

        protected void CustomMsaaEnd(SpriteBatch spriteBatch, RenderTargetBinding[]? rememberBindings)
        {
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
    }
}
