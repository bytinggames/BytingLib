using Microsoft.Xna.Framework.Content;

namespace BytingLib
{
    /// <summary>
    /// Supports chaining multiple raw content managers to only return the first successful asset load.
    /// Can be used for overriding the default content with a mod content.
    /// </summary>
    public class ContentManagerRawPipe : IContentManagerRaw
    {
        public readonly List<IContentManagerRaw> contentManagers;

        public ContentManagerRawPipe(params IContentManagerRaw[] contentManagers)
        {
            if (contentManagers.Length == 0)
                throw new ArgumentException("There must be at least one contentManager given in the arguments.");

            this.contentManagers = contentManagers.ToList();
        }

        public string RootDirectory => contentManagers[0].RootDirectory;

        public void Dispose()
        {
            for (int i = 0; i < contentManagers.Count; i++)
            {
                contentManagers[i].Dispose();
            }
        }

        /// <exception cref="ContentLoadException"/>
        public T Load<T>(string assetName)
        {
            for (int i = 0; i < contentManagers.Count - 1; i++)
            {
                try
                {
                    return contentManagers[i].Load<T>(assetName);
                }
                catch (ContentLoadException)
                {
                }
            }

            return contentManagers.Last().Load<T>(assetName); // if this method throws an exception it is not catched, but passed to the calling function.
        }

        public void UnloadAsset(string assetName)
        {
            for (int i = 0; i < contentManagers.Count; i++)
            {
                contentManagers[i].UnloadAsset(assetName);
            }
        }
    }
}
