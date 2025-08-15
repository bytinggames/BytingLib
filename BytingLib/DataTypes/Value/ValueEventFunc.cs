namespace BytingLib
{
    public class ValueEventFunc<T> : IValueEvent<T>, IValue<T>
    {
        private readonly Func<T> get;
        private readonly Action<T> set;

        public T Value
        {
            get => get();
            set
            {
                if (!EqualityComparer<T>.Default.Equals(value, get()))
                {
                    set(value);
                    OnChange?.Invoke(value);
                }
            }
        }

        public ValueEventFunc(Func<T> get, Action<T> set)
        {
            this.get = get;
            this.set = set;
        }

        public event Action<T>? OnChange;

        public void TriggerOnChange()
        {
            OnChange?.Invoke(get());
        }
    }
}
