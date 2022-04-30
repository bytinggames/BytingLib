namespace BytingLib
{
    class AssetHolder
    {
        private readonly object asset;
        private readonly string assetName;
        private readonly Action<string> onUnusedTo0References;
        private readonly List<object> assetReferences = new List<object>();

        public AssetHolder(object asset, string assetName, Action<string> onUnusedTo0References)
        {
            this.asset = asset;
            this.assetName = assetName;
            this.onUnusedTo0References = onUnusedTo0References;
        }

        public AssetRef<T> Use<T>()
        {
            AssetRef<T> assetRef = new AssetRef<T>((T)asset, Unuse);
            assetReferences.Add(assetRef);

            return assetRef;
        }

        private void Unuse(object asset)
        {
            assetReferences.Remove(asset);
            if (assetReferences.Count == 0)
                onUnusedTo0References?.Invoke(assetName);
        }
    }
}
