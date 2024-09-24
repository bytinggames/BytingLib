
namespace BytingLib
{
    public class Ref<T> : IDisposable
    {
        private readonly Promise<T> pointerToValue;
        private readonly Action<Ref<T>>? onDispose;
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
    }
}
