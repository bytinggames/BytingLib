namespace BytingLib.UI
{
    /// <summary>
    /// Does not scale the ui, but resizes to fit the screen.
    /// </summary>
    public class CanvasFlex : Canvas, IDrawBatch
    {
        public CanvasFlex(Func<Rect> getRenderRect, MouseInput mouse, KeyInput keys, GameWindow window, StyleRoot style) : base(getRenderRect, mouse, keys, window, style)
        {
        }

        public override void UpdateTree()
        {
            Rect rect = getRenderRect();

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
            BeforeDraw(spriteBatch);
            StyleRoot.SpriteBatchBegin = scissorTest =>
            {
                RasterizerState rs = scissorTest ? rasterizerStateScissor : rasterizerState;
                spriteBatch.Begin(rasterizerState: rs);
            };
            StyleRoot.SpriteBatchBegin(false);

            

            StyleRoot.SpriteBatchTransform = Matrix.Identity;
            StyleRoot.Push(Style);
            for (int i = 0; i < Children.Count; i++)
            {
                Children[i].Draw(spriteBatch, StyleRoot);
            }
            StyleRoot.Pop(Style);

            spriteBatch.End();
        }

        protected override void UpdateSelf(ElementInput input)
        {
            if (TreeDirty)
            {
                UpdateTree();
            }

            base.UpdateSelf(input);
        }
    }
}
