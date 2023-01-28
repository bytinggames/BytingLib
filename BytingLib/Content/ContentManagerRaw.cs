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
        /// <exception cref="ContentLoadException"/>
        public override T Load<T>(string assetName) => Load<T>(assetName, null);

        /// <summary>Foces the asset to be loaded from disc.</summary>
        /// <exception cref="ContentLoadException"/>
        public T Load<T>(string assetName, ExtendedLoadParameter? extendedLoad)
        {
            T asset;
            if (typeof(T) == typeof(AnimationData))
                asset = LoadAnimationData<T>(assetName);
            else if (typeof(T) == typeof(string))
                asset = (T)(object)LoadText(assetName);
            else if (typeof(T) == typeof(ModelGL))
            {
                if (extendedLoad == null)
                    throw new BytingException("Couldn't load ModelGL. Use Load(assetName, contentCollector) instead.");
                asset = (T)(object)LoadModelGL(assetName, extendedLoad.Value);
            }
            else if (typeof(T) == typeof(byte[]))
                asset = (T)(object)LoadByteArray(assetName);
            else
                asset = ReadAsset<T>(assetName, null);

            LoadedAssets.TryAdd(assetName, asset);
            return asset;
        }

        private T LoadAnimationData<T>(string assetName)
        {
            string filePath = Path.Combine(RootDirectory, assetName.Replace('/',  Path.DirectorySeparatorChar));
            if (!File.Exists(filePath))
                throw new ContentLoadException("file " + filePath + " does not exist");

            string json = File.ReadAllText(filePath);
            return (T)(object)AnimationData.FromJson(json);
        }

        private string LoadText(string assetNameWithExtension)
        {
            string filePath = Path.Combine(RootDirectory, assetNameWithExtension.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(filePath))
                throw new ContentLoadException("file " + filePath + " does not exist");

            return File.ReadAllText(filePath);
        }

        private byte[] LoadByteArray(string assetNameWithExtension)
        {
            string filePath = Path.Combine(RootDirectory, assetNameWithExtension.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(filePath))
                throw new ContentLoadException("file " + filePath + " does not exist");

            return File.ReadAllBytes(filePath);
        }

        private ModelGL LoadModelGL(string assetName, ExtendedLoadParameter extendedLoad)
        {
            string filePath = Path.Combine(RootDirectory, assetName.Replace('/', Path.DirectorySeparatorChar)) + ".gltf";
            if (!File.Exists(filePath))
                throw new ContentLoadException("file " + filePath + " does not exist");

            //throw new NotImplementedException();
            return new ModelGL(filePath, RootDirectory, extendedLoad.GraphicsDevice, extendedLoad.ContentCollector);
        }

        /// <summary>Forces the asset to be unloaded from RAM.</summary>
        public override void UnloadAsset(string assetName)
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
