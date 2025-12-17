namespace BytingLib
{
    public class StringEditToUpperInvariant : IStringEdit
    {
        private readonly Predicate<char>? when;

        public StringEditToUpperInvariant(Predicate<char>? when = null)
        {
            this.when = when;
        }

        public string GetApplied(string str)
        {
            if (when == null)
            {
                return str.ToUpperInvariant();
            }
            else
            {
                string strOutput = "";
                for (int i = 0; i < str.Length; i++)
                {
                    if (when(str[i]))
                    {
                        strOutput += char.ToUpperInvariant(str[i]);
                    }
                    else
                    {
                        strOutput += str[i];
                    }
                }
                return strOutput;
            }
        }
    }
}