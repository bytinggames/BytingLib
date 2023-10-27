using BytingLib;
using System.Text.RegularExpressions;

namespace BuildTemplates
{
    public class CustomContent
    {
        private string customHeader = "";
        private readonly List<(string Pattern, string BuildCode)> customContent = new();

        public CustomContent(string contentDirectory)
        {
            // read Content.Generated.custom
            string customFile = Path.Combine(contentDirectory, "Content.Generated.custom");
            if (!File.Exists(customFile))
            {
                // try find BytingLib Custom Content
                customFile = Path.Combine(customFile, "..", "..", "..", "BytingLib", "BytingLib", "Content", "Content.Generated.custom");
                if (!File.Exists(customFile))
                {
                    return;
                }
            }

            ReadFile(customFile);
        }

        public string GetCustomHeader() => customHeader;

        void ReadFile(string customFile)
        {
            string customStr = File.ReadAllText(customFile);

            // source: https://stackoverflow.com/a/3524689/6866837
            customStr = StripComments(customStr);

            customStr = customStr.Replace("\r", "");


            ScriptReader reader = new ScriptReader(customStr);

            while (true)
            {
                char? foundChar;
                string pattern = reader.ReadToCharOrEnd(out foundChar, '\n');
                if (foundChar != '\n')
                    break;

                if (pattern == "")
                    continue;

                if (pattern.StartsWith('@'))
                {
                    const string startsWithInclude = "@include \"";
                    const string startsWithHeader = "@header";
                    if (pattern.StartsWith(startsWithInclude))
                    {
                        int endIndex = pattern.IndexOf('"', startsWithInclude.Length);
                        string includeFile = pattern.Substring(startsWithInclude.Length, endIndex - startsWithInclude.Length);
                        includeFile = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(customFile)!, includeFile));
                        ReadFile(includeFile);
                    }
                    else if (pattern.StartsWith(startsWithHeader))
                    {
                        customHeader += reader.ReadToStringOrEnd("\n\n", out bool endReached1) + "\n";

                        if (endReached1)
                            break;
                    }
                    continue;
                }

                string buildCode = reader.ReadToStringOrEnd("\n\n", out bool endReached);
                customContent.Add((pattern, buildCode));

                if (endReached)
                    break;
            }
        }

        private static string StripComments(string customStr)
        {
            var blockComments = @"/\*(.*?)\*/";
            var lineComments = @"//(.*?)\r?\n";
            var strings = @"""((\\[^\n]|[^""\n])*)""";
            var verbatimStrings = @"@(""[^""]*"")+";

            string noComments = Regex.Replace(customStr,
                blockComments + "|" + lineComments + "|" + strings + "|" + verbatimStrings,
                me =>
                {
                    if (me.Value.StartsWith("/*") || me.Value.StartsWith("//"))
                        return me.Value.StartsWith("//") ? Environment.NewLine : "";
                    // Keep the literal strings
                    return me.Value;
                },
                RegexOptions.Singleline);
            return noComments;
        }

        internal string? GetCustomCode(string localFilePath)
        {
            for (int i = customContent.Count - 1; i >= 0; i--)
            {
                string pattern = customContent[i].Pattern;
                if (IsMatch(localFilePath, pattern))
                {
                    string code = customContent[i].BuildCode;
                    code = code.Replace("{source}", "{0}")
                        .Replace("{source-1}", "{1}")
                        .Replace("{ext}", "{2}")
                        .Replace("{source-2}", "{3}")
                        ;
                    int extIndex = localFilePath.LastIndexOf('.');
                    string localFilePathWithoutExtension;
                    string localFilePathWithoutTwoExtensions;
                    string extension = "";
                    if (extIndex != -1)
                    {
                        localFilePathWithoutExtension = localFilePath.Remove(extIndex);
                        extension = localFilePath.Substring(extIndex + 1);
                        extIndex = localFilePathWithoutExtension.LastIndexOf('.');
                        if (extIndex != -1)
                            localFilePathWithoutTwoExtensions = localFilePathWithoutExtension.Remove(extIndex);
                        else
                            localFilePathWithoutTwoExtensions = localFilePathWithoutExtension;
                    }
                    else
                        localFilePathWithoutExtension = localFilePathWithoutTwoExtensions = localFilePath;
                    code = string.Format(code, localFilePath, localFilePathWithoutExtension, extension, localFilePathWithoutTwoExtensions);
                    return "\n" + code;
                }
            }
            return null;
        }

        private bool IsMatch(string localFilePath, string pattern)
        {
            // start with "/" to match folder selections in regex like "/Folder/"
            return Regex.IsMatch("/" + localFilePath, pattern);
        }
    }
}
