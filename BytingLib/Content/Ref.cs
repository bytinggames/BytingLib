
namespace BytingLib
{
    public class Ref<T> : IDisposable
    {
        private Promise<T> pointerToValue;
        private Action<Ref<T>>? onDispose;
        public event Action<Ref<T>>? OnReload;

        public T Value
        {
            get => pointerToValue.Value!;
            set => pointerToValue.Value = value;
        }

        public Ref(Promise<T> pointerToValue, Action<Ref<T>>? onDispose)
        {
            this.pointerToValue = pointerToValue;
            this.onDispose = onDispose;
        }

        public void Dispose()
        {
            onDispose?.Invoke(this);
        }

        internal void TriggerOnReload()
        {
            OnReload?.Invoke(this);
        }

        /// <summary>Load another asset, while using the same Ref that other objects may already reference. This calls Dispose() first, before swapping to new pointer.</summary>
        internal void Override(Promise<T> pointerToValue, Action<Ref<T>>? onDispose)
        {
            Dispose();
            this.pointerToValue = pointerToValue;
            this.onDispose = onDispose;
        }
    }
}
