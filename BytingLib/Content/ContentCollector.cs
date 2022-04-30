namespace BytingLib
{
    public class ContentCollector : IContentCollector
    {
        private readonly IContentManagerRaw content;
        private readonly string relativeAssetPath = "";
        private readonly Dictionary<string, AssetHolder> loadedAssets = new();

        public ContentCollector(IContentManagerRaw content)
        {
            this.content = content;

            if (!string.IsNullOrEmpty(relativeAssetPath))
            {
                if (!relativeAssetPath.EndsWith("/"))
                    relativeAssetPath += "/";
            }
        }

        public AssetRef<T> Use<T>(string assetName)
        {
            assetName = relativeAssetPath + assetName;

            AssetHolder? assetHolder;
            if (!loadedAssets.TryGetValue(assetName, out assetHolder))
            {
                assetHolder = new AssetHolder(content.Load<T>(assetName)!, assetName, Unuse);
                loadedAssets.Add(assetName, assetHolder);
            }

            AssetRef<T> asset = assetHolder.Use<T>();

            return asset;
        }

        private void Unuse(string absoluteAssetName)
        {
            loadedAssets.Remove(absoluteAssetName);
            content.UnloadAsset(absoluteAssetName);
        }

        public void Dispose()
        {
            string[] names = new string[loadedAssets.Count];
            loadedAssets.Keys.CopyTo(names, 0);
            foreach (var n in names)
            {
                Unuse(n);
            }
        }
    }
}
