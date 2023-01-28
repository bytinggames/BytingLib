namespace BytingLib
{
    public class DisposableContainer : IDisposable
    {
        protected readonly List<IDisposable> disposables;

        public DisposableContainer()
        {
            disposables = new List<IDisposable>();
        }

        public DisposableContainer(params IDisposable?[] disposables)
        {
            this.disposables = disposables.Where(f => f != null).ToList()!;
        }

        public T Use<T>(T disposable) where T : notnull, IDisposable
        {
            disposables.Add(disposable);
            return disposable;
        }

        /// <summary>Same as Use, with the difference, that null can be passed as disposable. In this case this method does nothing.</summary>
        public T? UseCheckNull<T>(T? disposable) where T : IDisposable
        {
            if (disposable == null)
                return disposable;
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
