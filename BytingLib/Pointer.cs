namespace BytingLib
{
    public class Pointer<T>
    {
        public T? Value { get; set; }

        public Pointer()
        {
            Value = default;
        }

        public Pointer(T value)
        {
            Value = value;
        }
    }
}
