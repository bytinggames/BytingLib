
namespace BytingLib
{
    public class Ref<T> : IDisposable
    {
        private readonly Pointer<T> pointerToValue;
        private readonly Action<Ref<T>>? onDispose;

        public T Value => pointerToValue.Value!;

        public Ref(Pointer<T> pointerToValue, Action<Ref<T>>? onDispose)
        {
            this.pointerToValue = pointerToValue;
            this.onDispose = onDispose;
        }

        public void Dispose()
        {
            onDispose?.Invoke(this);
        }
    }
}
