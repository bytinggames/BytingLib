using Microsoft.Xna.Framework.Input;

namespace BytingLib.UI
{
    public abstract class Canvas : Element, IUpdate, IDrawBatch
    {
        private Element? updateCatch;
        public Color? ClearColor { get; set; }
        protected readonly Func<Rect> getRenderRect;
        public ElementInput Input { get; }
        public StyleRoot StyleRoot { get; set; }

        public Canvas(Func<Rect> getRenderRect, MouseInput mouse, KeyInput keys, GameWindow window, StyleRoot style)
        {
            this.getRenderRect = getRenderRect;
            StyleRoot = style;
            Input = CreateElementInput(mouse, keys, window);
        }

        protected virtual ElementInput CreateElementInput(MouseInput mouse, KeyInput keys, GameWindow window)
        {
            return new ElementInput(mouse, keys, SetUpdateCatch, window);
        }

        public void Update()
        {
            Input.Mouse.Update();

            if (updateCatch != null)
            {
                updateCatch.Update(Input);
            }
            else
            {
                UpdateSelf(Input);

                for (int i = 0; i < Children.Count; i++)
                    Children[i].Update(Input);
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

        public abstract void UpdateTree();
    }
}
