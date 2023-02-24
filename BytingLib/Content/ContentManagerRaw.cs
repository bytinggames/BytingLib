using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Text.Json.Nodes;

namespace BytingLib
{

    /// <summary>
    /// Shouldn't be used to load content directly. Use <see cref="ContentCollector"/> as a wrapper instead.
    /// </summary>
    public class ContentManagerRaw : ContentManager, IContentManagerRaw
    {
        private bool disposed;
        private GraphicsDevice gDevice;

        public ContentManagerRaw(IServiceProvider serviceProvider, string rootDirectory) : base(serviceProvider, rootDirectory)
        {
            IGraphicsDeviceService g = (ServiceProvider.GetService(typeof(IGraphicsDeviceService))! as IGraphicsDeviceService)!;
            gDevice = g.GraphicsDevice;
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
                asset = LoadTexture<T>(assetName);

            LoadedAssets.TryAdd(assetName, asset);
            return asset;

        }

        private T LoadTexture<T>(string assetName)
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

        private T LoadAnimationData<T>(string assetNameWithExtension)
        {
            string json = File.ReadAllText(GetFullFilePath(assetNameWithExtension, ""));
            return (T)(object)AnimationData.FromJson(json);
        }

        private string LoadText(string assetNameWithExtension)
        {
            return File.ReadAllText(GetFullFilePath(assetNameWithExtension, ""));
        }

        private byte[] LoadByteArray(string assetNameWithExtension)
        {
            return File.ReadAllBytes(GetFullFilePath(assetNameWithExtension, ""));
        }

        private ModelGL LoadModelGL(string assetName, ExtendedLoadParameter extendedLoad)
        {
            return new ModelGL(GetFullFilePath(assetName, ".gltf"), RootDirectory, extendedLoad.GraphicsDevice, extendedLoad.ContentCollector);
        }

        private FileStream OpenFile(string assetName, string extension)
        {
            return (FileStream)TitleContainer.OpenStream(Path.Combine(RootDirectory, assetName) + extension);
        }

        // TODO: remove this method and replace all usages by reading from the stream (currently this stream is closed and a new identical one is opened afterwards...)
        private string GetFullFilePath(string assetName, string extension)
        {
            using (FileStream fs = OpenFile(assetName, extension))
            {
                return fs.Name;
            }
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
