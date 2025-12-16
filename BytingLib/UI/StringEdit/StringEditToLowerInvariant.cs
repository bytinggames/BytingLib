namespace BytingLib
{
    public class StringEditToLowerInvariant : IStringEdit
    {
        public string GetApplied(string str)
        {
            return str.ToLowerInvariant();
        }
    }
}