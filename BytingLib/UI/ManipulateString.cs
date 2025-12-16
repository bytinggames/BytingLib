namespace BytingLib
{
    public enum ManipulateString
    {
        None,
        UpperCaseAll,
        LowerCaseAll
    }

    public static class ManipulateStringExtension
    {
        public static void Manipulate(this ManipulateString manipulate, ref string str)
        {
            switch (manipulate)
            {
                case ManipulateString.None:
                default:
                    break;
                case ManipulateString.UpperCaseAll:
                    str = str.ToUpperInvariant();
                    break;
                case ManipulateString.LowerCaseAll:
                    str = str.ToLowerInvariant();
                    break;
            }
        }
    }
}