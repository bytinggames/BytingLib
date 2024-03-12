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

        void ReplaceIncludes(ref string code, string path)
        {
            const string find = "@include \"";
            int index = 0;
            while ((index = code.IndexOf(find, index)) != -1)
            {
                index += find.Length;

                int endIndex = code.IndexOf('"', index);
                string includeFile = code.Substring(index, endIndex - index);
                includeFile = Path.GetFullPath(Path.Combine(path, includeFile));

                string innerCode = GetFileContents(includeFile);

                ReplaceIncludes(ref innerCode, Path.GetDirectoryName(includeFile)!);

                code = code.Remove(index - find.Length) + innerCode + code.Substring(endIndex + 1);
            }
        }

        string GetFileContents(string file)
        {
            string customStr = File.ReadAllText(file);

            customStr = StripComments(customStr);

            customStr = customStr.Replace("\r", "");
            return customStr;
        }

        void ReadFile(string customFile)
        {
            string customStr = GetFileContents(customFile);

            // replace @include "..."
            ReplaceIncludes(ref customStr, Path.GetDirectoryName(customFile)!);

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
                    const string startsWithHeader = "@header";
                    if (pattern.StartsWith(startsWithHeader))
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
            // source: https://stackoverflow.com/a/3524689/6866837
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
