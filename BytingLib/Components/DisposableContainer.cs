namespace BytingLib
{
    public class DisposableContainer : IDisposable
    {
        protected readonly List<IDisposable> disposables = new List<IDisposable>();

        protected T Use<T>(T disposable) where T : IDisposable
        {
            disposables.Add(disposable);
            return disposable;
        }

        public virtual void Dispose()
        {
            while (disposables.Count > 0)
            {
                disposables[^1].Dispose();
                disposables.RemoveAt(disposables.Count - 1);
            }
        }
    }
}
