using Microsoft.Xna.Framework.Graphics;

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

        public Ref<string> UseString(string assetNameWithExtension)
        {
            return UseCustom(assetNameWithExtension, fileName => File.ReadAllText(fileName));
        }

        public Ref<byte[]> UseBytes(string assetNameWithExtension)
        {
            return UseCustom(assetNameWithExtension, fileName => File.ReadAllBytes(fileName));
        }

        private Ref<T> UseCustom<T>(string assetNameWithExtension, Func<string, T> readAsset)
        {
            string assetName = relativeAssetPath + assetNameWithExtension;

            object? assetHolder;
            if (!loadedAssets.TryGetValue(assetName, out assetHolder))
            {
                T assetContent = readAsset(Path.Combine(contentRaw.RootDirectory, assetName));
                assetHolder = new AssetHolder<T>(assetContent, assetName, Unuse);
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

        public AssetHolder<T>? GetAssetHolder<T>(string assetName)
        {
            assetName = relativeAssetPath + assetName;

            object? assetHolder;
            if (!loadedAssets.TryGetValue(assetName, out assetHolder))
                return null;

            return assetHolder as AssetHolder<T>;
        }

        ///// <summary>If the asset is already loaded, it returns it. If not it retuns null.</summary>
        //public T? Peek<T>(string assetName)
        //{
        //    var assetHolder = GetAssetHolder<T>(assetName);
        //    if (assetHolder == null)
        //        return default;
        //    return assetHolder.Peek();
        //}

        //public T? ReloadIfLoaded<T>(string assetName)
        //{
        //    var assetHolder = GetAssetHolder<T>(assetName);
        //    if (assetHolder == null)
        //        return default;

        //    contentRaw.UnloadAsset(assetName);
        //    T asset = contentRaw.Load<T>(assetName);

        //    assetHolder!.Replace(asset);

        //    return asset;
        //}

        public void ReloadLoadedAsset<T>(AssetHolder<T> assetHolder)
        {
            if (assetHolder is null) throw new ArgumentNullException(nameof(assetHolder));

            contentRaw.UnloadAsset(assetHolder.AssetName);
            T asset = contentRaw.Load<T>(assetHolder.AssetName);

            assetHolder.Replace(asset);
        }

        //public void ReplaceAsset<T>(string assetName, T newValue)
        //{
        //    var assetHolder = GetAssetHolder<T>(assetName);
        //    if (assetHolder == null)
        //        return;
        //    assetHolder.Replace(newValue);
        //}
    }
}
