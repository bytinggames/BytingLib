namespace BytingLib
{
    class AssetHolder<T>
    {
        private readonly Pointer<T> assetPointer;
        private readonly string assetName;
        private readonly Action<string> onUnusedTo0References;
        private readonly List<Ref<T>> assetReferences = new List<Ref<T>>();

        public AssetHolder(T asset, string assetName, Action<string> onUnusedTo0References)
        {
            assetPointer = new Pointer<T>(asset);
            this.assetName = assetName;
            this.onUnusedTo0References = onUnusedTo0References;
        }

        public Ref<T> Use()
        {
            Ref<T> assetRef = new Ref<T>(assetPointer, Unuse);
            assetReferences.Add(assetRef);

            return assetRef;
        }

        private void Unuse(Ref<T> asset)
        {
            assetReferences.Remove(asset);
            if (assetReferences.Count == 0)
                onUnusedTo0References?.Invoke(assetName);
        }

        public T Seek()
        {
            return assetPointer.Value!;
        }

        internal void Replace(T newValue)
        {
            if (assetPointer.Value is IDisposable disposable)
                disposable.Dispose();

            assetPointer.Value = newValue;
        }
    }
}
