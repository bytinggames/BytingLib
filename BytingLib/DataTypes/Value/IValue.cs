namespace BytingLib
{
    public interface IValue<T> : IValueGet<T>, IValueSet<T>
    {
        public new T Value { get; set; }
    }
}
