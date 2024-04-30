namespace BytingLib
{
    public interface IValueSet<T> where T : struct
    {
        public T Value { set; }
    }
}
