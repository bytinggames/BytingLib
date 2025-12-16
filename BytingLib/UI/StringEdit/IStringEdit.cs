using System.Diagnostics.CodeAnalysis;

namespace BytingLib
{
    public interface IStringEdit
    {
        public string GetApplied(string str);
    }
    public static class IStringEditExtension
    {
        public static bool Exists([NotNullWhen(true)] this IStringEdit? edit)
        {
            return edit != null && edit is not StringEditResetToNone;
        }

        public static void Apply(this IStringEdit? edit, ref string str)
        {
            if (edit != null)
            {
                str = edit.GetApplied(str);
            }
        }
    }
}