namespace BytingLib
{
    public class ValueEvent<T> : IValueEvent<T>, IValueGet<T>, IValueSet<T>, IValue<T>
    {
        private T _value;
        public T Value
        {
            get => _value;
            set
            {
                if (!EqualityComparer<T>.Default.Equals(value, _value))
                {
                    _value = value;
                    OnChange?.Invoke(value);
                }
            }
        }

        /// <summary>
        /// This constructor shouldn't be used when T is a class type.
        /// As then, Value is null, but it should never be null.
        /// This constructor exists only for json deserialization</summary>
        [Obsolete]
        internal ValueEvent()
        {
            _value = default!;
        }

        public ValueEvent(T val)
        {
            _value = val;
        }

        public event Action<T>? OnChange;

        public void TriggerOnChange()
        {
            OnChange?.Invoke(_value);
        }
    }
}
