namespace BytingLib
{
    public interface IValueEvent<T> : IValue<T>
    {
        public event Action<T>? OnChange;
        public void TriggerOnChange();
    }
}
