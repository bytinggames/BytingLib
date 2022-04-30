namespace BytingLib
{
    public interface IContentCollector : IDisposable
    {
        AssetRef<T> Use<T>(string assetName);
    }
}