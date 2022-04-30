namespace BytingLib
{
    public interface IContentManagerRaw : IDisposable
    {
        T Load<T>(string assetName);
        void UnloadAsset(string assetName);
    }
}