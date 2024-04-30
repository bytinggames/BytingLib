namespace BytingLib
{
    public interface IValue<T> : IValueGet<T>, IValueSet<T> where T : struct
    {
        public new T Value { get; set; }
    }
}
