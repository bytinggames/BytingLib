namespace BytingLib
{
    public class AssetRef<T> : IDisposable
    {
        public T Asset { get; }
        private readonly Action<object> onDispose;

        public AssetRef(T asset, Action<object> onDispose)
        {
            Asset = asset;
            this.onDispose = onDispose;
        }

        public void Dispose()
        {
            onDispose?.Invoke(this);
        }
    }
}
