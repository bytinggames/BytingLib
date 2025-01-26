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
        public string Path { get; } = path;

        public Ref<T> Use() => d.Use<T>(Path);
        public void Override(Ref<T> asset) => d.Override(Path, asset);
    }
}
