namespace BytingLib
{
    public interface IContentCollector : IDisposable
    {
        Ref<T> Use<T>(string assetName);
        AssetHolder<T>? GetAssetHolder<T>(string assetName);
        public void ReloadLoadedAsset<T>(AssetHolder<T> assetHolder);
    }
}