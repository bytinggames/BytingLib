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
        private Element? focusedElement;
        public Action? OnEnterWhenUnfocused { get; set; }
        public ICanvasFocus? FocusManager { get; set; }
        public bool FocusIfUnfocused { get; set; }
        public event Action? OnFocusStart;
        public Vector2? FocusStartNearPosition { get; set; }

        //private bool scissorTest;
        protected readonly RasterizerState rasterizerState = CreateDefaultRasterizerState();
        protected readonly RasterizerState rasterizerStateScissor;

        public Element? FocusedElement
        {
            get => focusedElement;
            set
            {
                if (focusedElement == null && value != null)
                {
                    OnFocusStart?.Invoke();
                }
                focusedElement = value;
            }
        }

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
            }

            UpdateNavigation();
        }

        private void UpdateNavigation()
        {
            if (FocusManager != null)
            {
                if (FocusStartNearPosition.HasValue)
                {
                    FocusNearest(FocusStartNearPosition.Value);

                    FocusStartNearPosition = null;
                    FocusIfUnfocused = false;
                }

                if (FocusIfUnfocused)
                {
                    FocusIfUnfocused = false;
                    if (FocusedElement == null)
                    {
                        Navigate(Vector2.Zero);
                    }
                }

                if (Input.Input.Navigate.Value != Vector2.Zero)
                {
                    Vector2 navigate = Input.Input.Navigate.Value;
                    Navigate(navigate);
                }
            }

            if (Input.Input.Enter.Pressed)
            {
                if (FocusedElement == null)
                {
                    OnEnterWhenUnfocused?.Invoke();
                }
                else
                {
                    if (FocusManager != null
                        && FocusedElement is ICanFocus canFocus)
                    {
                        canFocus.ClickFromFocus();
                    }
                }
            }
        }

        public void Navigate(Vector2 navigate)
        {
            // if no element is focused yet, see if a child provides a starting point
            Element? navigateFrom = null;
            if (FocusedElement == null)
            {
                navigateFrom = GetAllVisibleChildren().FirstOrDefault(f => f.NavigationStart != UINavigationStart.None);
                if (navigateFrom != null 
                    && (navigateFrom.NavigationStart == UINavigationStart.ToThisElement || navigate == Vector2.Zero))// if navigation is zero, it means we navigate to the marked navigation start
                {
                    FocusedElement = navigateFrom;
                    return;
                }
                // if we didn't find a start element, make sure to start from the center and since we have to navigate into some direction, choose upwards
                if (navigate == Vector2.Zero)
                {
                    navigate = new Vector2(0f, -1f);
                }
            }
            else if (navigate == Vector2.Zero)
            {
                return; // direction required
            }

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
            Rect focusRect = FocusedElement?.AbsoluteRect 
                ?? navigateFrom?.AbsoluteRect
                ?? new Rect(this.AbsoluteRect.GetCenter(), Vector2.One);

            focusRect = focusRect.CloneRect();
            focusRect.Grow(-2f); // make a bit smaller so there's always 1px distance to next ui element, even if stacked without spacing

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
            foreach (var child in (updateCatch ?? this).GetAllVisibleChildren().OfType<ICanFocus>().Where(f => f.CanFocus))
            {
                if (child == FocusedElement)
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

                float centerDistOnDirection = Vector2.Dot(navigate, centerDist);
                float myScore = 0f;

                if (centerDistOnDirection == 0f)
                {
                    // makes no sense to go to completely orthogonal element
                    continue;
                }

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
                    myScore -= 100000f; // score penalty for screen wrapping. They compete in their own category and only have a chance if only screen wrappers compete.
                }

                float inDirection = Vector2.Dot(Vector2.Normalize(dist + centerDist * 0.01f), navigate);
                inDirection = MathF.Pow(inDirection, screenWrap ? 1f : 4f); // make it more unlikely to move orthogonally
                myScore -= (dist.Length() + centerDist.Length() * 0.1f /* not as important. more of a tie breaker */) / inDirection;

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

        public void FocusNearest(Vector2 focus)
        {
            ICanFocus? nearest = GetAllVisibleChildren()
                .Where(f => f.AbsoluteRect != null)
                .OfType<ICanFocus>()
                .Where(f => f.CanFocus)
                .MinBy(f => ((Element)f).AbsoluteRect.DistanceToPoint(focus).LengthSquared());
            if (nearest != null)
            {
                FocusedElement = (Element)nearest;
            }
        }
    }
}
