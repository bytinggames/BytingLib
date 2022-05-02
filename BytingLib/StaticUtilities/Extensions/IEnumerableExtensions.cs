namespace BytingLib
{
    public static class IEnumerableExtensions
    {
        public static IEnumerable<T> ForEvery<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (var f in source)
            {
                action(f);
            }
            return source;
        }
    }
}
