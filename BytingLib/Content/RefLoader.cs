using System.IO;

namespace BytingLib
{
    public record RefLoaderDependencies(IContentCollector Collector, DisposableContainer Disposables)
    {
        public Ref<T> Use<T>(string assetPath)
        {
            return Disposables.Use(Collector.Use<T>(assetPath));
        }
        public void Override<T>(string assetPath, Ref<T> asset)
        {
            Collector.Override(assetPath, asset);
        }
    }

    public class RefLoader<T>(RefLoaderDependencies d, string path)
    {
        private readonly RefLoaderDependencies d = d;
        private readonly string path = path;

        public Ref<T> Use() => d.Use<T>(path);
        public void Override(Ref<T> asset) => d.Override(path, asset);
    }
}
