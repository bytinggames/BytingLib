namespace BytingLib
{
    /// <summary>This is a struct for performance reasons (allocating many instances of this struct)</summary>
    public struct OnDispose : IDisposable
    {
        private readonly Action onDisposeAction;

        public OnDispose(Action onDisposeAction)
        {
            this.onDisposeAction = onDisposeAction ?? throw new ArgumentNullException(nameof(onDisposeAction));
        }

        public void Dispose()
        {
            onDisposeAction.Invoke();
        }
    }
}
