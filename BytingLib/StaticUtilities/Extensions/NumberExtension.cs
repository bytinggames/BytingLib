using System.Numerics;

namespace BytingLib
{
    public static class NumberExtension
    {
        public static T AddClamped<T>(this T a, T b)
            where T : INumber<T>, IMinMaxValue<T>
        {
            return AddClamped(a, b, T.MaxValue);
        }

        public static T AddClamped<T>(this T a, T b, T max)
            where T : INumber<T>, IMinMaxValue<T>
        {
            return max - a < b
                ? max
                : a + b;
        }
    }
}
