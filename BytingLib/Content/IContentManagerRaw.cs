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
            string dir = Path.Combine(contentManager.RootDirectory, assetName);
            if (!Directory.Exists(dir))
                return false;
            return true;
            // this would not work for loading pngs for example
            //if (File.Exists(dir + ".xnb"))
            //    return true;
        }
    }
}