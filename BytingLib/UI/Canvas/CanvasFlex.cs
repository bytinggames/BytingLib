﻿namespace BytingLib.UI
{
    public class CanvasFlex : Canvas, IDrawBatch, IUpdate
    {
        public CanvasFlex(Func<Rect> getRenderRect, MouseInput mouse, Style style) : base(getRenderRect, mouse, style)
        {
        }

        public void UpdateTree()
        {
            Rect rect = getRenderRect();

            Width = rect.Width;
            Height = rect.Height;

            absoluteRect = rect.CloneRect().Round();

            for (int i = 0; i < Children.Count; i++)
            {
                Children[i].UpdateTree(rect);
            }
        }

        public void DrawBatch(SpriteBatch spriteBatch)
        {
            UpdateTree(); // TODO: only update tree when necessary

            if (ClearColor != null)
                spriteBatch.GraphicsDevice.Clear(ClearColor.Value);

            spriteBatch.Begin();

            for (int i = 0; i < Children.Count; i++)
            {
                Children[i].Draw(spriteBatch, Style);
            }

            spriteBatch.End();
        }

        protected override void UpdateSelf(ElementInput input)
        {
            UpdateTree(); // TODO: only update tree when necessary
        }
    }
}
