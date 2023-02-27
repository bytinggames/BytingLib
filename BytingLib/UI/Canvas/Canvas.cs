namespace BytingLib.UI
{
    public abstract class Canvas : Element, IUpdate
    {
        private Element? updateCatch;
        public Color? ClearColor { get; set; }
        protected readonly Func<Rect> getRenderRect;
        protected readonly ElementInput input;

        public Canvas(Func<Rect> getRenderRect, MouseInput mouse)
        {
            this.getRenderRect = getRenderRect;
            input = CreateElementInput(mouse);
        }

        protected virtual ElementInput CreateElementInput(MouseInput mouse)
        {
            return new ElementInput(mouse, SetUpdateCatch);
        }

        public void Update()
        {
            input.Mouse.Update();

            if (updateCatch != null)
            {
                updateCatch.Update(input);
            }
            else
            {
                UpdateSelf(input);

                for (int i = 0; i < Children.Count; i++)
                    Children[i].Update(input);
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

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            throw new BytingException("Call DrawBatch() instead");
        }
    }
}
