namespace BytingLib
{
    public class ContentCollector : IContentCollector
    {
        private readonly IContentManagerRaw contentRaw;
        private readonly string relativeAssetPath = "";
        private readonly Dictionary<string, object> loadedAssets = new(); // dictionary of AssetHolder<T>, but unknown T

        public ContentCollector(IContentManagerRaw content)
        {
            this.contentRaw = content;

            if (!string.IsNullOrEmpty(relativeAssetPath))
            {
                if (!relativeAssetPath.EndsWith("/"))
                    relativeAssetPath += "/";
            }
        }

        public Ref<T> Use<T>(string assetName)
        {
            assetName = relativeAssetPath + assetName;

            object? assetHolder;
            if (!loadedAssets.TryGetValue(assetName, out assetHolder))
            {
                assetHolder = new AssetHolder<T>(contentRaw.Load<T>(assetName)!, assetName, Unuse);
                loadedAssets.Add(assetName, assetHolder);
            }

            Ref<T> asset = (assetHolder as AssetHolder<T>)!.Use();

            return asset;
        }

        private void Unuse(string absoluteAssetName)
        {
            loadedAssets.Remove(absoluteAssetName);
            contentRaw.UnloadAsset(absoluteAssetName);
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

        /// <summary>If the asset is already loaded, it returns it. If not it retuns null.</summary>
        public T? Seek<T>(string assetName)
        {
            assetName = relativeAssetPath + assetName;

            object? assetHolder;
            if (!loadedAssets.TryGetValue(assetName, out assetHolder))
                return default;

            return (assetHolder as AssetHolder<T>)!.Seek();
        }

        public T? ReloadIfLoaded<T>(string assetName)
        {
            assetName = relativeAssetPath + assetName;

            object? assetHolder;
            if (!loadedAssets.TryGetValue(assetName, out assetHolder))
                return default;

            contentRaw.UnloadAsset(assetName);
            T asset = contentRaw.Load<T>(assetName);

            (assetHolder as AssetHolder<T>)!.Replace(asset);

            return asset;
        }

        public void ReplaceAsset<T>(string assetName, T newValue)
        {
            assetName = relativeAssetPath + assetName;

            object? assetHolder;
            if (!loadedAssets.TryGetValue(assetName, out assetHolder))
                return;

            (assetHolder as AssetHolder<T>)!.Replace(newValue);
        }
    }
}
