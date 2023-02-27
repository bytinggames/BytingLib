namespace BytingLib.UI
{
    public class CanvasFlex : Element, IDrawBatch
    {
        private readonly Func<Rect> getRenderRect;
        private readonly Color? clearColor;


        public CanvasFlex(Func<Rect> getRenderRect, Color? clearColor = null)
        {
            this.getRenderRect = getRenderRect;
            this.clearColor = clearColor;
        }

        protected virtual void SpriteBatchBegin(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();
        }

        public void DrawBatch(SpriteBatch spriteBatch)
        {
            if (clearColor != null)
                spriteBatch.GraphicsDevice.Clear(clearColor.Value);

            Rect rect = getRenderRect();

            Width = rect.Width;
            Height = rect.Height;

            SpriteBatchBegin(spriteBatch);

            for (int i = 0; i < Children.Count; i++)
            {
                Children[i].Draw(spriteBatch, rect);
            }

            spriteBatch.End();
        }

        public override void Draw(SpriteBatch spriteBatch, Rect parentRect)
        {
            throw new BytingException("Call DrawBatch() instead");
        }
    }
}
