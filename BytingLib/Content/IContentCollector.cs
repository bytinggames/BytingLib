namespace BytingLib
{
    public interface IContentCollector : IDisposable
    {
        Ref<T> Use<T>(string assetName);
        T? Seek<T>(string assetName);
        void ReplaceAsset<T>(string assetName, T newValue);
        T? ReloadIfLoaded<T>(string assetName);
    }
}