namespace BytingLib
{
    public class StringEditReplace : IStringEdit
    {
        Dictionary<char, string> replace;

        public StringEditReplace(string constructByLine)
        {
            replace = new();
            string[] split = constructByLine.Split(['\n']);
            for (int i = 0; i < split.Length; i++)
            {
                if (split[i].Length >= 2)
                {
                    replace.Add(split[i][0], split[i].Substring(1));
                }
            }
        }
        public StringEditReplace(Dictionary<char, string> replace)
        {
            this.replace = replace;
        }

        public string GetApplied(string str)
        {
            string output = "";
            for (int i = 0; i < str.Length; i++)
            {
                if (replace.TryGetValue(str[i], out string? replaceWith))
                {
                    output += replaceWith;
                }
                else
                {
                    output += str[i];
                }
            }
            return output;
        }
    }
}
