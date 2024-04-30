using System.Globalization;

namespace BytingLib
{
    public static class StringExtension
    {
        public static bool TryParse(this string? str, out double value) => double.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out value);
        public static bool TryParse(this string? str, out float value) => float.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out value);
    }
}
