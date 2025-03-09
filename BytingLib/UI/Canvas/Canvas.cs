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
        public Matrix Transform { get; protected set; } = Matrix.Identity;
        public Element? FocusedElement { get; set; }
        public ICanvasFocus? FocusManager { get; set; }

        //private bool scissorTest;
        protected readonly RasterizerState rasterizerState = CreateDefaultRasterizerState();
        protected readonly RasterizerState rasterizerStateScissor;

        public Canvas(Func<Rect> getRenderRect, IInputCanvas input, GameWindow window, StyleRoot style)
        {
            this.getRenderRect = getRenderRect;
            StyleRoot = style;
            Input = CreateElementInput(input, window);

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

        protected virtual ElementInput CreateElementInput(IInputCanvas input, GameWindow window)
        {
            return new ElementInput(input, SetUpdateCatch, UnsetUpdateCatch, window);
        }

        public void Update()
        {
            // reset hover element
            Input.HoverElement = null;

            if (updateCatch != null)
            {
                updateCatch.Update(Input);
            }
            else
            {
                UpdateSelf(Input);

                for (int i = Children.Count - 1; i >= 0; i--)
                {
                    Children[i].Update(Input);
                }

                if (Input.Input.Navigate.Value != Vector2.Zero)
                {
                    Vector2 navigate = Input.Input.Navigate.Value;
                    Navigate(navigate);
                }

                if (Input.Input.Enter.Pressed)
                {
                    if (FocusedElement != null)
                    {
                        if (FocusedElement is ICanFocus canFocus)
                        {
                            canFocus.ClickFromFocus();
                        }
                    }
                }
            }
        }

        public void Navigate(Vector2 navigate)
        {
            if (MathF.Abs(navigate.X) >= 0.5f
                && FocusedElement is SliderInt slider)
            {
                slider.Value += navigate.X > 0 ? 1 : -1;
                return;
            }

            float bestScore = float.NegativeInfinity;
            Element? bestScoreElement = null;
            navigate.Normalize();
            Vector2 navigateOrth = new Vector2(-navigate.Y, navigate.X);
            bool currentlyFocused = FocusedElement != null;
            Rect focusRect = FocusedElement?.AbsoluteRect ?? new Rect(this.AbsoluteRect.GetCenter(), Vector2.One);
            Vector2 focusCenter = focusRect.GetCenter();
            Vector2 focusScreenWrapCenter = Vector2.Zero;
            Rect? focusRectScreenWrap = null;
            var cr = focusRect.DistanceTo(AbsoluteRect, -navigate);
            bool anyNonWrapperScored = false;
            if (cr.DistanceReversed.HasValue)
            {
                focusRectScreenWrap = focusRect.CloneRect();
                focusRectScreenWrap.Pos -= navigate * cr.DistanceReversed.Value;
                focusScreenWrapCenter = focusRectScreenWrap.GetCenter();
            }
            foreach (var child in GetAllVisibleChildren().OfType<ICanFocus>())
            {
                if (!child.CanFocus
                    || child == FocusedElement)
                {
                    continue;
                }

                Element element = (Element)child;
                if (element.AbsoluteRect == null)
                {
                    continue;
                }
                Vector2 dist = focusRect.DistanceToRect(element.AbsoluteRect);
                Vector2 centerDist = element.AbsoluteRect.GetCenter() - focusCenter;

                float distOnDirection = Vector2.Dot(navigate, dist);
                float centerDistOnDirection = Vector2.Dot(navigate, centerDist);
                float myScore = 0f;
                bool screenWrap = centerDistOnDirection <= 0f;
                if (screenWrap)
                {
                    // wrong direction
                    // try to wrap around the screen, but with a much worse score
                    if (anyNonWrapperScored || focusRectScreenWrap == null)
                    {
                        // no chance
                        continue;
                    }
                    dist = focusRectScreenWrap.DistanceToRect(element.AbsoluteRect);
                    centerDist = element.AbsoluteRect.GetCenter() - focusScreenWrapCenter;
                    centerDistOnDirection = Vector2.Dot(navigate, centerDist);
                    distOnDirection = Vector2.Dot(navigate, dist);
                    myScore -= 100000f; // score penalty for screen wrapping. They compete in their own category and only have a chance if only screen wrappers compete.
                }

                float orthogonalDistance = MathF.Abs(Vector2.Dot(navigateOrth, dist));
                if (orthogonalDistance > distOnDirection
                    && currentlyFocused) // if nothing is focused, take the next best thing to focus
                {
                    // more to the the orthogonal direction than to the correct direction
                    continue;
                }

                float orthogonalCenterDistance = MathF.Abs(Vector2.Dot(navigateOrth, centerDist));

                myScore -= distOnDirection
                    + MathF.Abs(centerDistOnDirection) * 0.01f // just in case elements are overlapping
                    + orthogonalDistance * 2f + orthogonalCenterDistance * 0.5f;

                if (myScore > bestScore)
                {
                    bestScore = myScore;
                    bestScoreElement = element;

                    if (!screenWrap)
                    {
                        anyNonWrapperScored = true;
                    }
                }
            }

            if (bestScoreElement != null)
            {
                FocusedElement = bestScoreElement;
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
        public void UnsetUpdateCatch(Element element)
        {
            if (element == updateCatch)
            {
                updateCatch = null;
            }
        }

        public abstract void DrawBatch(SpriteBatch spriteBatch);

        protected void DrawCanvasBase(SpriteBatch spriteBatch)
        {
            if (FocusedElement != null && FocusManager != null)
            {
                FocusManager.Draw(spriteBatch, FocusedElement);
            }
        }

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
