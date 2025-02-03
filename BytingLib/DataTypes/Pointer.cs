namespace BytingLib
{
    public class Pointer<T> : IPointerValue
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

        public void SetPointerValue(object obj)
        {
            if (obj is T val)
            {
                Value = val;
            }
        }
        public Type GetDeclaredPointerValueType() => typeof(T);
        public object? GetPointerValue() => Value;

        public override string ToString()
        {
            return Value?.ToString() ?? "";
        }
    }
}
