namespace BytingLib
{
    public interface IContentCollector : IDisposable
    {
        Ref<T> Use<T>(string assetName);
        Ref<string> UseString(string assetNameWithExtension);
        Ref<byte[]> UseBytes(string assetNameWithExtension);
        AssetHolder<T>? GetAssetHolder<T>(string assetName);
        public void ReloadLoadedAsset<T>(AssetHolder<T> assetHolder);
        public void SubscribeToOnLoad<T>(string assetName, Action<T> onLoadAction);
        public bool UnsubscribeToOnLoad<T>(string assetName, Action<T> onLoadAction);
        public void SubscribeToOnLoad(string assetName, Action onLoadAction);
        public bool UnsubscribeToOnLoad(string assetName, Action onLoadAction);
        public void TryTriggerOnLoad<T>(string assetName, T asset);
    }
}