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
        public Ref<Effect>? Effect { get; set; }
        protected Rect? LastRenderRect { get; private set; }

        //private bool scissorTest;
        protected readonly RasterizerState rasterizerState = CreateDefaultRasterizerState();
        protected readonly RasterizerState rasterizerStateScissor;

        public Canvas(Func<Rect> getRenderRect, MouseInput mouse, KeyInput keys, GameWindow window, StyleRoot style)
        {
            this.getRenderRect = getRenderRect;
            StyleRoot = style;
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

        protected void SetDirtyIfResChanged()
        {
            Rect newRenderRect = getRenderRect();
            if (!LastRenderRect.EqualValue(newRenderRect))
            {
                SetDirty();
                LastRenderRect = newRenderRect;
            }
        }
    }
}
