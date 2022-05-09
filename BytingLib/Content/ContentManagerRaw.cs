using Microsoft.Xna.Framework.Content;

namespace BytingLib
{
    /// <summary>
    /// Shouldn't be used to load content directly. Use <see cref="ContentCollector"/> as a wrapper instead.
    /// </summary>
    public class ContentManagerRaw : ContentManager, IContentManagerRaw
    {
        private bool disposed;

        public ContentManagerRaw(IServiceProvider serviceProvider, string rootDirectory) : base(serviceProvider, rootDirectory)
        {
        }

        /// <summary>Foces the asset to be loaded from disc.</summary>
        public override T Load<T>(string assetName)
        {
            if (typeof(T) == typeof(AnimationData))
            {
                string json = File.ReadAllText(Path.Combine(RootDirectory, assetName.Replace("/", "\\") + ".json"));
                return (T)(object)AnimationData.FromJson(json);
            }

            T asset = ReadAsset<T>(assetName, null);
            LoadedAssets.Add(assetName, asset);
            return asset;
        }

        /// <summary>Foces the asset to be unloaded from RAM.</summary>
        public void UnloadAsset(string assetName)
        {
            if (string.IsNullOrEmpty(assetName))
            {
                throw new ArgumentNullException(nameof(assetName));
            }
            if (disposed)
            {
                throw new ObjectDisposedException("ContentManager");
            }

            //Check if the asset exists
            object? asset;
            if (LoadedAssets.TryGetValue(assetName, out asset))
            {
                AssetDisposer.Dispose(asset);

                LoadedAssets.Remove(assetName);
            }
        }

        public new void Dispose()
        {
            foreach (var asset in LoadedAssets)
            {
                if (asset.Value is IDisposable disposable)
                    disposable.Dispose();
            }
            LoadedAssets.Clear();

            base.Dispose();

            disposed = true;
        }
    }
}
