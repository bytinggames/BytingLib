using BytingLib;

namespace LevelSketch
{
    public class StructSource<T> : IUpdate, IDisposable where T : struct
    {
        private readonly Func<T> getStateDefault;
        private IEnumerator<T>? inputSource;
        private Action<IEnumerator<T>>? onSourceRemove;

        public event Action<T>? OnUpdate;

        public StructSource(Func<T> getState)
        {
            this.getStateDefault = getState ?? throw new ArgumentNullException(nameof(getState));
        }

        public void Update()
        {
            if (inputSource != null)
            {
                if (!inputSource.MoveNext())
                    RemoveSource();
            }

            Current = inputSource?.Current ?? getStateDefault();

            OnUpdate?.Invoke(Current);
        }

        public T Current { get; private set; }

        public void SetSource(IEnumerator<T>? source, Action<IEnumerator<T>>? onSourceRemove)
        {
            var oldSource = inputSource;
            var oldOnEnumeratorRemove = this.onSourceRemove;

            this.onSourceRemove = onSourceRemove;
            inputSource = source;

            if (oldSource != null)
                oldOnEnumeratorRemove?.Invoke(oldSource);
        }

        public void Dispose()
        {
            RemoveSource();
        }

        public void RemoveSource()
        {
            SetSource(null, null);
        }
    }
}
