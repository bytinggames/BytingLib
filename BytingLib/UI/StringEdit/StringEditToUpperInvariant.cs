namespace BytingLib
{
    public class StringEditToUpperInvariant : IStringEdit
    {
        public string GetApplied(string str)
        {
            return str.ToUpperInvariant();
        }
    }
}