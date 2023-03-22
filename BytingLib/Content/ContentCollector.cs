namespace BytingLib
{
    public class ContentCollector : IContentCollector
    {
        private readonly IContentManagerRaw contentRaw;
        private readonly string relativeAssetPath = "";
        private readonly Dictionary<string, object> loadedAssets = new(); // dictionary of AssetHolder<T>, but unknown T
        private readonly Dictionary<string, Dictionary<object, Action<object>>> onLoad = new();
        private readonly ExtendedLoadParameter extendedLoad;

        public ContentCollector(IContentManagerRaw content, GraphicsDevice gDevice)
        {
            this.contentRaw = content;

            if (!string.IsNullOrEmpty(relativeAssetPath))
            {
                if (!relativeAssetPath.EndsWith("/"))
                    relativeAssetPath += "/";
            }

            extendedLoad = new(this, gDevice);
        }

        private string ToTotalAssetName(string assetName)
        {
            assetName = relativeAssetPath + assetName;
            return assetName;
        }

        public Ref<T> Use<T>(string assetName)
        {
            assetName = ToTotalAssetName(assetName);
            bool triggerOnLoad = false;
            object? assetHolder;
            if (!loadedAssets.TryGetValue(assetName, out assetHolder))
            {
                assetHolder = new AssetHolder<T>(contentRaw.Load<T>(assetName, extendedLoad), assetName, Unuse);
                loadedAssets.Add(assetName, assetHolder);

                if (onLoad.ContainsKey(assetName))
                    triggerOnLoad = true;
            }

            Ref<T> asset = (assetHolder as AssetHolder<T>)!.Use();
            if (triggerOnLoad && asset != null)
                TriggerOnLoad(assetName, asset.Value);

            return asset!;
        }

        public void TryTriggerOnLoad<T>(string assetName, T asset)
        {
            if (onLoad.ContainsKey(assetName) && asset != null)
                TriggerOnLoad<T>(assetName, asset);
        }

        private void TriggerOnLoad<T>(string assetName, T asset)
        {
            if (asset != null)
            {
                foreach (var action in onLoad[assetName].Values)
                {
                    action.Invoke(asset);
                }
            }
        }

        /// <summary>needs to be updated or removed alltogether (see TODO)</summary>
        public Ref<string> UseString(string assetNameWithExtension)
        {
            return UseCustom(assetNameWithExtension, fileName => File.ReadAllText(fileName));
        }

        /// <summary>needs to be updated or removed alltogether (see TODO)</summary>
        public Ref<byte[]> UseBytes(string assetNameWithExtension)
        {
            return UseCustom(assetNameWithExtension, fileName => File.ReadAllBytes(fileName));
        }

        /// <summary>needs to be updated or removed alltogether (see TODO)</summary>
        private Ref<T> UseCustom<T>(string assetNameWithExtension, Func<string, T> readAsset)
        {
            string assetName = relativeAssetPath + assetNameWithExtension;

            bool triggerOnLoad = false;
            object? assetHolder;
            if (!loadedAssets.TryGetValue(assetName, out assetHolder))
            {
                T assetContent = readAsset(Path.Combine(contentRaw.RootDirectory, assetName)); // TODO: currently this pulls only the RootDirectory of the first contnet manager, which could be the hot reload content manager. When this load fails, it fails alltogether. Fix this by iterating through the content manager root directories.
                assetHolder = new AssetHolder<T>(assetContent, assetName, Unuse);
                loadedAssets.Add(assetName, assetHolder);

                if (onLoad.ContainsKey(assetName))
                    triggerOnLoad = true;
            }

            Ref<T> asset = (assetHolder as AssetHolder<T>)!.Use();

            if (triggerOnLoad && asset.Value != null)
                TriggerOnLoad(assetName, asset.Value);

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
            T asset = contentRaw.Load<T>(assetHolder.AssetName, extendedLoad);

            assetHolder.Replace(asset);

            if (onLoad.ContainsKey(assetHolder.AssetName))
                TriggerOnLoad(assetHolder.AssetName, asset);
        }

        public void SubscribeToOnLoad<T>(string assetName, Action<T> onLoadAction)
        {
            assetName = ToTotalAssetName(assetName);

            if (!onLoad.ContainsKey(assetName))
                onLoad.Add(assetName, new Dictionary<object, Action<object>>());
            else if (onLoad[assetName].ContainsKey(onLoadAction))
                throw new Exception($"there is already a subscription on asset {assetName} with key {onLoadAction}");
            onLoad[assetName].Add(onLoadAction, obj => onLoadAction((T)obj));
        }

        public void SubscribeToOnLoad(string assetName, Action onLoadAction)
        {
            assetName = ToTotalAssetName(assetName);

            if (!onLoad.ContainsKey(assetName))
                onLoad.Add(assetName, new Dictionary<object, Action<object>>());
            else if (onLoad[assetName].ContainsKey(onLoadAction))
                throw new Exception($"there is already a subscription on asset {assetName} with key {onLoadAction}");
            onLoad[assetName].Add(onLoadAction, obj => onLoadAction());
        }

        public bool UnsubscribeToOnLoad<T>(string assetName, Action<T> onLoadAction)
        {
            return PrivateUnsubscribeToOnLoad(assetName, onLoadAction);
        }

        public bool UnsubscribeToOnLoad(string assetName, Action onLoadAction)
        {
            return PrivateUnsubscribeToOnLoad(assetName, onLoadAction);
        }

        private bool PrivateUnsubscribeToOnLoad(string assetName, object action)
        {
            assetName = ToTotalAssetName(assetName);

            if (onLoad.ContainsKey(assetName))
            {
                onLoad[assetName].Remove(action);

                if (onLoad[assetName].Count == 0)
                    onLoad.Remove(assetName);

                return true;
            }
            return false;
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
