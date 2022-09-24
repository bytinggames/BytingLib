using Microsoft.Xna.Framework.Content;

namespace BytingLib
{
    /// <summary>
    /// Supports chaining multiple raw content managers to only return the first successful asset load.
    /// Can be used for overriding the default content with a mod content.
    /// </summary>
    public class ContentManagerRawPipe : IContentManagerRaw
    {
        public readonly List<IContentManagerRaw> ContentManagers;

        public ContentManagerRawPipe(params IContentManagerRaw[] contentManagers)
        {
            if (contentManagers.Length == 0)
                throw new ArgumentException("There must be at least one contentManager given in the arguments.");

            this.ContentManagers = contentManagers.ToList();
        }

        public string RootDirectory => ContentManagers[0].RootDirectory;

        public void Dispose()
        {
            for (int i = 0; i < ContentManagers.Count; i++)
            {
                ContentManagers[i].Dispose();
            }
        }

        /// <exception cref="ContentLoadException"/>
        public T Load<T>(string assetName)
        {
            for (int i = 0; i < ContentManagers.Count - 1; i++)
            {
                try
                {
                    return ContentManagers[i].Load<T>(assetName);
                }
                catch (ContentLoadException)
                {
                }
            }

            return ContentManagers.Last().Load<T>(assetName); // if this method throws an exception it is not catched, but passed to the calling function.
        }

        public void UnloadAsset(string assetName)
        {
            for (int i = 0; i < ContentManagers.Count; i++)
            {
                ContentManagers[i].UnloadAsset(assetName);
            }
        }
    }
}
