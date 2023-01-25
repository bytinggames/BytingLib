namespace BytingLib
{
    /// <summary>Used for rendering whole sprite batches using SpriteBatch.Begin() and SpriteBatch.End()</summary>
    public interface IDrawBatch
    {
        public void DrawBatch(SpriteBatch spriteBatch);
    }
}
