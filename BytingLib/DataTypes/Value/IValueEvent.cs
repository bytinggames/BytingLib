namespace BytingLib
{
    public interface IValueEvent<T> : IValue<T> where T : struct
    {
        public event Action<T>? OnChange;
        public void TriggerOnChange();
    }
}
