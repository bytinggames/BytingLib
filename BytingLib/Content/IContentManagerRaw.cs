namespace BytingLib
{
    public interface IContentManagerRaw : IDisposable
    {
        T Load<T>(string assetName, ExtendedLoadParameter? extendedLoad);
        void UnloadAsset(string assetName);
        string RootDirectory { get; }
    }

    public static class IContentManagerRawExtension
    {
        /// <summary>
        /// Only checks if the directory for that assets exists
        /// </summary>
        public static bool MightBeAbleToLoad(this IContentManagerRaw contentManager, string assetName)
        {
            string? localDir = Path.GetDirectoryName(assetName);
            string dir;
            if (localDir == null)
                dir = contentManager.RootDirectory;
            else
                dir = Path.Combine(contentManager.RootDirectory, localDir);

            if (!Directory.Exists(dir))
                return false;
            return true;
        }
    }
}