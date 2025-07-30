namespace BytingLib.Markup
{
    public static class Markup
    {
        public static string Escape(string str)
        {
            return $"#escape({str.Length}|{str})";
        }
    }
}
