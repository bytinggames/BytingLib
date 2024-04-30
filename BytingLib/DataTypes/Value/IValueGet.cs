namespace BytingLib
{
    public interface IValueGet<T> where T : struct
    {
        public T Value { get; }
    }
}
