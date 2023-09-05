namespace BytingLib
{
    public class ValueEvent<T> where T : struct
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

        public ValueEvent() { }

        public ValueEvent(T val)
        {
            _value = val;
        }

        public event Action<T>? OnChange;
    }
}
