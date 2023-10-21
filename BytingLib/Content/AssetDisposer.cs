namespace BytingLib
{
    public static class AssetDisposer
    {
        /// <summary>Calls IDisposable.Dispose() and SpriteFont.Texture.Dispose() if existing</summary>
        public static void Dispose(object? asset)
        {
            PreDispose(asset);

            if (asset is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
        /// <summary>Calls SpriteFont.Texture.Dispose() if existing</summary>
        public static void PreDispose(object? asset)
        {
            if (asset is SpriteFont font)
            {
                font.Texture.Dispose();
            }
        }
    }
}
