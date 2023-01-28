namespace BytingLib
{
    public interface IContentManagerRaw : IDisposable
    {
        T Load<T>(string assetName, ExtendedLoadParameter? extendedLoad);
        void UnloadAsset(string assetName);
        string RootDirectory { get; }
    }
}