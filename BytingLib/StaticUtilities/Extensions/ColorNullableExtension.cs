using System.Diagnostics.CodeAnalysis;

namespace BytingLib.UI
{
    public static class ColorNullableExtension
    {
        public static bool IsNotTransparent([NotNullWhen(true)] this Color? color)
        {
            return color != null && color != Color.Transparent;
        }
    }
}
