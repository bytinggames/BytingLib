using System.Numerics;

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

        public static bool IsSumAtLeast<T, TSum>(this IEnumerable<T> source, Func<T, TSum> getSum, TSum threshold) where TSum : INumber<TSum>
        {
            if (threshold <= TSum.Zero)
            {
                return true;
            }

            foreach (var item in source)
            {
                threshold -= getSum(item);
                if (threshold <= TSum.Zero)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
