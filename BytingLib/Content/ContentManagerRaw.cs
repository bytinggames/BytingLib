using Microsoft.Xna.Framework.Content;

namespace BytingLib
{

    /// <summary>
    /// Shouldn't be used to load content directly. Use <see cref="ContentCollector"/> as a wrapper instead.
    /// </summary>
    public class ContentManagerRaw : ContentManager, IContentManagerRaw
    {
        //private bool disposed;
        private GraphicsDevice gDevice;

        public ContentManagerRaw(IServiceProvider serviceProvider, string rootDirectory) : base(serviceProvider, rootDirectory)
        {
            IGraphicsDeviceService g = (ServiceProvider.GetService(typeof(IGraphicsDeviceService))! as IGraphicsDeviceService)!;
            gDevice = g.GraphicsDevice;
        }


        /// <summary>Forces the asset to be loaded from disc.</summary>
        /// <exception cref="ContentLoadException"/>
        public override T Load<T>(string assetName) => Load<T>(assetName, null);

        /// <summary>Forces the asset to be loaded from disc.</summary>
        /// <exception cref="ContentLoadException"/>
        public T Load<T>(string assetName, ExtendedLoadParameter? extendedLoad)
        {
            T asset = LoadInner<T>(assetName);
            LoadedAssets.TryAdd(assetName, asset);
            return asset;

        }

        private T LoadInner<T>(string assetName)
        {
            try
            {
                return ReadAsset<T>(assetName, null);
            }
            catch (Exception)
            {
                // this currently tries to load a png or jpg, if the xnb file was not found
                // could be improved though, so that this code immediately knows, wether to search for a png jpg or an xnb
                if (typeof(T) == typeof(Texture2D))
                {
                    bool notFound;
                    string assetFile = Path.Combine(RootDirectory, assetName) + ".png";
                    try
                    {
                        return Load(assetFile);
                    }
                    catch
                    {

                        assetFile = Path.Combine(RootDirectory, assetName) + ".jpg";
                        try
                        {
                            return Load(assetFile);
                        }
                        catch
                        {
                            notFound = true;
                        }
                    }

                    if (notFound)
                        throw;

                    T Load(string assetFile)
                    {
                        using (Stream stream = TitleContainer.OpenStream(assetFile))
                        {
                            var tex = Texture2D.FromStream(gDevice, stream);
                            return (T)Convert.ChangeType(tex, typeof(T));
                        }
                    }
                }
                else
                    throw;
            }
            return default!; // just to silence the compiler
        }

        // disabled the custom dispose code, cause it creates a memory leak
        // I'm not sure if spritefont textures are disposed correctly though, cause that's the reason I implemented this AssetDisposer.Dispose(asset) section
        // TODO: it should be tested if SpriteFont textures are disposed correctly
        ///// <summary>Forces the asset to be unloaded from RAM.</summary>
        //public override void UnloadAsset(string assetName)
        //{
        //    if (string.IsNullOrEmpty(assetName))
        //    {
        //        throw new ArgumentNullException(nameof(assetName));
        //    }
        //    if (disposed)
        //    {
        //        throw new ObjectDisposedException("ContentManager");
        //    }

        //    //Check if the asset exists
        //    object? asset;
        //    if (LoadedAssets.TryGetValue(assetName, out asset))
        //    {
        //        AssetDisposer.Dispose(asset);

        ////this is not possible like in the base UnloadAsset method. Therefore a memory leak is created...
        ////        if (asset is IDisposable disposable)
        ////            disposableAssets.Remove(disposable);

        //        LoadedAssets.Remove(assetName);
        //    }
        //}

        //public new void Dispose()
        //{
        //    foreach (var asset in LoadedAssets)
        //    {
        //        AssetDisposer.Dispose(asset.Value);
        //    }
        //    LoadedAssets.Clear();

        //    base.Dispose();

        //    disposed = true;
        //}
    }
}
