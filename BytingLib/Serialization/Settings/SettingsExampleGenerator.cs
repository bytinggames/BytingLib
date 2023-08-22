using YamlDotNet.Serialization;

namespace BytingLib
{
    internal class SettingsExampleGenerator
    {
        public static string GenerateYaml(object settings, string settingsCSFile)
        {
            var comments = LoadComments(settingsCSFile);

            ISerializer serializer = new SerializerBuilder()
                .WithEmissionPhaseObjectGraphVisitor(f => new CommentsObjectGraphVisitor(comments, f.InnerVisitor))
                .Build();


            string yamlTotal = "\r\n# Regenerate this file by deleting it and restarting the game.\r\n\r\n";
            
            string yaml = serializer.Serialize(settings);
            yaml = yaml.Replace("\r\n\r\n", "\r\n"); // remove empty lines
            yaml += "\r\n";
            yamlTotal += yaml;

            return yamlTotal;
        }

        private static Dictionary<string, string> LoadComments(string file)
        {
            Dictionary<string, string> comments = new Dictionary<string, string>();

            if (!File.Exists(file))
            {
                return comments;
            }

            string code = File.ReadAllText(Path.Combine(@"..\..\..\pl5m1bu7", file));
            int index = 0;

            while (true)
            {
                const string summaryStart = "/// <summary>";
                const string summaryEnd = "</summary>";

                int start = code.IndexOf(summaryStart, index);
                if (start == -1)
                {
                    break;
                }
                start += summaryStart.Length;
                int end = code.IndexOf(summaryEnd, start);
                if (end == -1)
                {
                    continue;
                }

                string comment = code.Substring(start, end - start);
                comment = comment
                    .Replace("///", "")
                    .Replace("\r\n", "")
                    .Replace("  ", " ")
                    .Replace("\"", "\\\"");
                comment = comment.Trim();

                end += summaryEnd.Length;
                index = end;

                // find next '{'
                end = code.IndexOf('{', index);
                index = end - 1;
                // find recent non-whitespace
                while (char.IsWhiteSpace(code[index]))
                {
                    index--;
                }
                end = index + 1;
                // find recent whitespace
                while (!char.IsWhiteSpace(code[index]))
                {
                    index--;
                }
                start = index + 1;
                string propertyName = code.Substring(start, end - start);

                comments.Add(propertyName, comment);

                index = end + 2; // behind that '{'
            }

            return comments;
        }
    }
}
