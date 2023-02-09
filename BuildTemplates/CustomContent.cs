using BytingLib;
using System.Text.RegularExpressions;

namespace BuildTemplates
{
    public class CustomContent
    {
        List<(string Command, string Pattern, string BuildCode)> customContent = new();

        public CustomContent(string contentDirectory)
        {
            // read Content.Generated.custom
            string customPath = Path.Combine(contentDirectory, "Content.Generated.custom");
            if (File.Exists(customPath))
            {
                string customStr = File.ReadAllText(customPath);
                customStr = customStr.Replace("\r", "");
                ScriptReader reader = new ScriptReader(customStr);

                while (true)
                {
                    char? foundChar;
                    string command = reader.ReadToCharOrEnd(out foundChar, ':');
                    if (foundChar != ':')
                        break;
                    string pattern = reader.ReadToCharOrEnd(out foundChar, '\n');
                    if (foundChar != '\n')
                        break;
                    string buildCode = reader.ReadToStringOrEnd("\n\n", out bool endReached);
                    customContent.Add((command, pattern, buildCode));

                    if (endReached)
                        break;
                }
            }
        }

        internal bool GetCustomCode(string localFilePath, ref string buildProcess, ref string command)
        {
            for (int i = 0; i < customContent.Count; i++)
            {
                string pattern = customContent[i].Pattern;
                if (IsMatch(localFilePath, pattern))
                {
                    buildProcess = "\n" + customContent[i].BuildCode;
                    command = customContent[i].Command;
                    return true;
                }
            }
            return false;
        }

        private bool IsMatch(string localFilePath, string pattern)
        {
            return Regex.IsMatch(localFilePath, pattern);
        }
    }
}
