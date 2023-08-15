namespace BytingLib.UI
{
    /// <summary>
    /// Does not scale the ui, but resizes to fit the screen.
    /// </summary>
    public class CanvasFlex : Canvas, IDrawBatch
    {
        public CanvasFlex(Func<Rect> getRenderRect, MouseInput mouse, StyleRoot style) : base(getRenderRect, mouse, style)
        {
        }

        public override void UpdateTree()
        {
            Rect rect = getRenderRect();

            if (Padding != null)
                rect.ApplyPadding(Padding.Left, Padding.Right, Padding.Top, Padding.Bottom);

            Width = rect.Width;
            Height = rect.Height;

            AbsoluteRect = rect.CloneRect().Round();

            StyleRoot.Push(Style);
            for (int i = 0; i < Children.Count; i++)
                Children[i].UpdateTreeBegin(StyleRoot);
            for (int i = 0; i < Children.Count; i++)
                Children[i].UpdateTree(rect);
            StyleRoot.Pop(Style);
        }

        public void DrawBatch(SpriteBatch spriteBatch)
        {
            UpdateTree(); // TODO: only update tree when necessary

            if (ClearColor != null)
                spriteBatch.GraphicsDevice.Clear(ClearColor.Value);

            spriteBatch.Begin();

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
            UpdateTree(); // TODO: only update tree when necessary
        }
    }
}
