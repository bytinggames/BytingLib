namespace BytingLib.UI
{
    public abstract class Canvas : Element, IUpdate, IDrawBatch
    {
        private Element? updateCatch;
        public Color? ClearColor { get; set; }
        protected readonly Func<Rect> getRenderRect;
        protected readonly WindowManager windowManager;

        public ElementInput Input { get; }
        public StyleRoot StyleRoot { get; set; }
        private bool treeDirty = true;
        public Ref<Effect>? Effect { get; set; }
        protected Rect? LastRenderRect { get; private set; }
        public Matrix Transform { get; protected set; } = Matrix.Identity;
        private Element? navigateElement;
        public Action? OnEnterWithoutNavigation { get; set; }
        public INavigationDrawer? NavigationDrawer { get; set; }
        public bool BeginNavigation { get; set; }
        public event Action? OnNavigationStart;
        public Vector2? NavigateToPosition { get; set; }
        public event Action<Element>? OnNavigate;
        public bool AllowNavigation { get; set; } = true;
        private bool firstUpdate = true;

        //private bool scissorTest;
        protected readonly RasterizerState rasterizerState = CreateDefaultRasterizerState();
        protected readonly RasterizerState rasterizerStateScissor;

        public Element? NavigateElement
        {
            get => navigateElement;
            set
            {
                if (navigateElement != null)
                {
                    navigateElement.Hover = false;
                }

                if (navigateElement == null && value != null)
                {
                    OnNavigationStart?.Invoke();
                }
                navigateElement = value;

                if (navigateElement != null)
                {
                    navigateElement.Hover = true;
                }
            }
        }

        public Canvas(Func<Rect> getRenderRect, IInputCanvas input, WindowManager windowManager, StyleRoot style, GameSpeed updateSpeed)
        {
            this.getRenderRect = getRenderRect;
            this.windowManager = windowManager;
            StyleRoot = style;
            Input = CreateElementInput(input, windowManager, updateSpeed);

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

        protected virtual ElementInput CreateElementInput(IInputCanvas input, WindowManager windowManager, GameSpeed updateSpeed)
        {
            return new ElementInput(input, SetUpdateCatch, UnsetUpdateCatch, windowManager.Window, updateSpeed);
        }

        public void Update()
        {
            if (firstUpdate)
            {
                firstUpdate = false;
                // update navigation once, to prioritize navigation over dead mouse hover
                UpdateNavigation();
            }

            // reset hover element
            Input.HoverElement = null;
            Input.NavigateElement = NavigateElement;

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

            NavigateElement = Input.NavigateElement;
            UpdateNavigation();
        }

        private void UpdateNavigation()
        {
            if (!AllowNavigation)
            {
                return;
            }

            if (NavigationDrawer != null)
            {
                if (NavigateToPosition.HasValue)
                {
                    NavigateToPositionNow(NavigateToPosition.Value);

                    NavigateToPosition = null;
                    BeginNavigation = false;
                }

                if (BeginNavigation)
                {
                    BeginNavigation = false;
                    if (NavigateElement == null)
                    {
                        Navigate(Vector2.Zero, false);
                    }
                }

                if (Input.Input.Navigate.Value != Vector2.Zero)
                {
                    Vector2 navigate = Input.Input.Navigate.Value;
                    Navigate(navigate, false);
                }
                if (Input.Input.NavigateWithLetters.Value != Vector2.Zero)
                {
                    Vector2 navigate = Input.Input.NavigateWithLetters.Value;
                    Navigate(navigate, true);
                }
            }

            if (Input.Input.Enter.Pressed)
            {
                if (NavigateElement == null)
                {
                    OnEnterWithoutNavigation?.Invoke();
                }
                else
                {
                    if (NavigationDrawer != null
                        && NavigateElement is ICanBeNavigated canNavigate)
                    {
                        canNavigate.ActivateFromNavigation();
                    }
                }
            }
        }

        public void Navigate(Vector2 navigate, bool navigateWithLetters)
        {
            Element? navigateTo = NavigateInner(navigate, navigateWithLetters);

            if (navigateTo != null)
            {
                NavigateElement = navigateTo;

                if (NavigateElement is TextInput textInput2)
                {
                    Input.FocusElement = textInput2;
                }

                var scroll = navigateTo.GetParents().OfType<PanelScroll>().FirstOrDefault();
                if (scroll != null)
                {
                    if (!navigateTo.AbsoluteRect.IsEnclosedIn(scroll.AbsoluteRect))
                    {
                        float scrollBy = 0f;
                        if (navigateTo.AbsoluteRect.Top < scroll.AbsoluteRect.Top)
                        {
                            scrollBy = navigateTo.AbsoluteRect.Top - scroll.AbsoluteRect.Top;
                        }
                        else if (navigateTo.AbsoluteRect.Bottom > scroll.AbsoluteRect.Bottom)
                        {
                            scrollBy = navigateTo.AbsoluteRect.Bottom - scroll.AbsoluteRect.Bottom;
                        }
                        if (scrollBy != 0f)
                        {
                            scroll.Scroll += scrollBy;
                        }
                    }
                }

                OnNavigate?.Invoke(NavigateElement);
            }
        }

        private Element? NavigateInner(Vector2 navigate, bool navigateWithLetters)
        {
            // if no element is navigated to yet, see if a child provides a starting point
            Element? navigateFrom = null;
            if (NavigateElement == null)
            {
                if (Input.FocusElement is TextInput)
                {
                    if (MathF.Abs(navigate.X) >= 0.5f
                        || navigateWithLetters)
                    {
                        return null;
                    }
                }

                if (Input.HoverElement != null
                    && Input.HoverElement.CanBeNavigated)
                {
                    navigateFrom = Input.HoverElement;
                }
                else
                {
                    navigateFrom = GetAllVisibleChildren()
                        .Where(f => (f is not IEnabled enabled || enabled.Enabled) && f.NavigationStart != UINavigationStart.None && f.CanBeNavigated)
                        .MaxBy(f => f.NavigationStartPriority);
                }
                if (navigateFrom != null
                    && (navigateFrom.NavigationStart == UINavigationStart.ToThisElement || navigate == Vector2.Zero))// if navigation is zero, it means we navigate to the marked navigation start
                {
                    return navigateFrom;
                }
                // if we didn't find a start element, make sure to start from the center and since we have to navigate into some direction, choose upwards
                if (navigate == Vector2.Zero)
                {
                    navigate = new Vector2(0f, -1f);
                }
            }
            else if (navigate == Vector2.Zero)
            {
                return null; // direction required
            }

            if (MathF.Abs(navigate.X) >= 0.5f
                && NavigateElement is SliderInt slider)
            {
                slider.Value += navigate.X > 0 ? 1 : -1;
                return null;
            }
            if (NavigateElement is TextInput textInput)
            {
                if (navigateWithLetters)
                {
                    return null;
                }
                if (MathF.Abs(navigate.X) >= 0.5f)
                {
                    return null;
                }
                else
                {
                    if (Input.FocusElement == NavigateElement)
                    {
                        Input.FocusElement = null;
                    }
                    textInput.LooseFocus();
                }
            }

            float bestScore = float.NegativeInfinity;
            Element? bestScoreElement = null;
            navigate.Normalize();
            Vector2 navigateOrth = new Vector2(-navigate.Y, navigate.X);
            bool currentlyNavigating = NavigateElement != null;
            Rect navigateRect = NavigateElement?.AbsoluteRect
                ?? navigateFrom?.AbsoluteRect
                ?? new Rect(this.AbsoluteRect.GetCenter(), Vector2.One);

            navigateRect = navigateRect.CloneRect();
            navigateRect.Grow(-2f); // make a bit smaller so there's always 1px distance to next ui element, even if stacked without spacing

            Vector2 navigateCenter = navigateRect.GetCenter();
            Vector2 navigateScreenWrapCenter = Vector2.Zero;
            Rect? navigateRectScreenWrap = null;
            var cr = navigateRect.DistanceTo(AbsoluteRect, -navigate);
            bool anyNonWrapperScored = false;
            if (cr.DistanceReversed.HasValue)
            {
                navigateRectScreenWrap = navigateRect.CloneRect();
                navigateRectScreenWrap.Pos -= navigate * cr.DistanceReversed.Value;
                navigateRectScreenWrap.Pos -= navigate * this.AbsoluteRect.Size.Length(); // move 1 screen further away, to punish wrapping over bounds
                navigateScreenWrapCenter = navigateRectScreenWrap.GetCenter();
            }
            foreach (var child in (updateCatch ?? this).GetAllVisibleChildren().OfType<ICanBeNavigated>().Where(f => f.CanBeNavigated))
            {
                if (child == NavigateElement)
                {
                    continue;
                }

                Element element = (Element)child;
                if (element.AbsoluteRect == null
                    || !element.IsRectangleVisible()) // check if element is inside a scroll element and not visible
                {
                    continue;
                }

                Vector2 dist = navigateRect.DistanceToRect(element.AbsoluteRect);
                Vector2 centerDist = element.AbsoluteRect.GetCenter() - navigateCenter;

                float centerDistOnDirection = Vector2.Dot(navigate, centerDist);
                float myScore = 0f;

                if (centerDistOnDirection == 0f)
                {
                    // makes no sense to go to completely orthogonal element
                    continue;
                }

                bool screenWrap = Vector2.Dot(dist, navigate) <= 0f;

                if (screenWrap)
                {
                    // wrong direction
                    // try to wrap around the screen, but with a much worse score
                    if (anyNonWrapperScored || navigateRectScreenWrap == null)
                    {
                        // no chance
                        continue;
                    }
                    dist = navigateRectScreenWrap.DistanceToRect(element.AbsoluteRect);
                    centerDist = element.AbsoluteRect.GetCenter() - navigateScreenWrapCenter;
                }

                Vector2 n = Vector2.Normalize(dist + centerDist * 0.01f);
                if (screenWrap)
                {
                    // prioritize finding elements that are close to the border
                    // stretch the distance into the direction you are searching for. This will prioritize orthogonal elements -> closer to the border
                    dist *= Vector2.One + navigate.GetAbs() * 1f;
                }
                else
                {
                    // prioritize perfectly aligned elements
                    if (Vector2.Dot(dist, navigateOrth) == 0f)
                    {
                        if (MathF.Abs(dist.X) > MathF.Abs(dist.Y))
                        {
                            // horizontal movement
                            if (MathF.Abs(centerDist.Y) < 8f) // a leeway of 8px
                            {
                                // make distance appear closer
                                dist.X /= 4f;
                                centerDist.X /= 4f;
                            }
                        }
                        else
                        {
                            // vertical movement
                            if (MathF.Abs(centerDist.X) < 8f) // a leeway of 8px
                            {
                                // make distance appear closer
                                dist.Y /= 4f;
                                centerDist.Y /= 4f;
                            }
                        }
                    }
                }
                myScore -= dist.Length() + (navigateOrth * Vector2.Dot(navigateOrth, centerDist)).Length() * 0.1f /* not as important. more of a tie breaker */;

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

            return bestScoreElement;
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

        public abstract void DrawBatch(SpriteBatch spriteBatch, float extrapolation);

        protected void DrawCanvasBase(SpriteBatch spriteBatch)
        {
            if (NavigateElement != null && NavigationDrawer != null)
            {
                NavigationDrawer.Draw(spriteBatch, NavigateElement);
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

        /// <summary>Warning: some AbsoluteRects might not be initialized. When unsure, use NavigateToNearest property instead.</summary>
        public void NavigateToPositionNow(Vector2 atPosition)
        {
            ICanBeNavigated? nearest = GetAllVisibleChildren()
                .Where(f => f.AbsoluteRect != null)
                .OfType<ICanBeNavigated>()
                .Where(f => f.CanBeNavigated)
                .MinBy(f => ((Element)f).AbsoluteRect.DistanceToPoint(atPosition).LengthSquared());
            if (nearest != null)
            {
                NavigateElement = (Element)nearest;
            }
        }
    }
}
