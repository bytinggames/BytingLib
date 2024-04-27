using BytingLib;

namespace BuildTemplates
{
    public class LocaGenerator
    {
        const string TAB = "    ";

        private static string GenerateFromFile(string csvFile)
        {
            Localization loca = new Localization(csvFile, "en");
            Dictionary<string, string> dictionary = loca.GetDictionary();

            return GenerateLoca(dictionary);
        }

        //public static string Generate(string contentPath, string nameSpace)
        //{
        //    string locaCode = "";

        //    foreach (var file in Directory.EnumerateFiles(contentPath, "*.loca", SearchOption.AllDirectories))
        //    {
        //        string csvContent = File.ReadAllText(file);
        //        string[] languages = csvContent.Remove(csvContent.IndexOf('\n')).Split(new char[] { ';' }).Skip(1).ToArray();

        //        //foreach (string language = languages[0];//)
        //        string language = languages[0];
        //        {
        //            var lan = language.Replace("\r", "");
        //            Localization loca = new Localization(csvContent, lan);
        //            Dictionary<string, string> dictionary = loca.GetDictionary();


        //            locaCode += GenerateLoca(dictionary, nameSpace);
        //                // WAIT: is hot reloading even possible then?
        //                // isn'T this overkill?
        //        }
        //    }

        //    return locaCode;
        //}

        class Folder
        {
            Dictionary<string, Folder> folders = new();
            public List<string> codeLines = new();
            public Folder? parent;
            public string className;

            public Folder(Folder? parent, string className)
            {
                this.parent = parent;
                string s = className[0].ToString();
                if (s.ToUpperInvariant() == s.ToLowerInvariant()) // is upper case the same?
                {
                    this.className = "_" + className;
                }
                else
                {
                    this.className = className[0].ToString().ToUpper() + className.Substring(1);
                }
                if (parent == null)
                {
                    codeLines.AddRange(new string[]{
                        "public static Dictionary<string, string> Dict;",
                        "public static Localization L;",
                        "public static void Initialize(string languageKey)",
                        "{",
                        TAB + $"L = new Localization(\"Content/{this.className}.loca\", languageKey);",
                        TAB + "Dict = L.GetDictionary();",
                        "}"
                    });
                }
            }

            internal Folder GetFolder(List<string> dirSplit)
            {
                if (dirSplit.Count <= 1)
                    return this;

                Folder? folder;
                if (!folders.TryGetValue(dirSplit[0], out folder))
                {
                    folder = new(this, dirSplit[0]);
                    folders.Add(dirSplit[0], folder);
                }

                dirSplit.RemoveAt(0);
                return folder.GetFolder(dirSplit);
            }

            internal string Print(string tabs)
            {
                string childOutput = "";
                foreach (var f in folders)
                {
                    childOutput += f.Value.Print(tabs + TAB);
                }
                string output =
$@"
{tabs}public class @{className}
{tabs}{{
{tabs}{TAB}{string.Join("\n" + tabs + TAB, codeLines)}{childOutput}
{tabs}}}";
                return output;
            }
        }

        static string GenerateLoca(Dictionary<string, string> dictionary)
        {

            Folder root = new(null, "Loca");

            foreach (var (key, value) in dictionary)
            {
                if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(value))
                    continue;

                // character position, number
                Dictionary<int, int> parameters = new Dictionary<int, int>();
                int index = 0;
                int maxParameterIndex = -1;
                while ((index = value.IndexOf("{", index)) != -1)
                {
                    index++;
                    char c = value[index];
                    if (c >= '0' && c <= '9')
                    {
                        int indexOfBracketClose = value.IndexOf("}", index + 1);
                        string numberStr = value.Substring(index, indexOfBracketClose - index);
                        int number = int.Parse(numberStr);
                        parameters.Add(index - 1, number);

                        if (number > maxParameterIndex)
                            maxParameterIndex = number;
                    }
                }

                List<string> dirSplit = key.Split(new char[] { '.' }).ToList();

                for (int i = 0; i < dirSplit.Count; i++)
                {
                    if (dirSplit[i][0] < 'A' || dirSplit[i][0] > 'z')
                        dirSplit[i] = "_" + dirSplit[i];
                }

                Folder currentFolder = root.GetFolder(dirSplit);
                string keyVariable = dirSplit[0];

                if (parameters.Count == 0)
                    currentFolder.codeLines.Add($"public static string {keyVariable} => Dict[\"{key}\"];");
                    //output += $"public const string {key} = \"{value}\";\n" + tabs;
                else
                {
                    string parameterStr = "";
                    string parameterPass = "";
                    for (int j = 0; j <= maxParameterIndex; j++)
                    {
                        parameterStr += (j > 0 ? ", " : "") + "string s" + j;
                        parameterPass += (j > 0 ? ", " : "") + "s" + j;
                    }

                    var sortedDict = from entry in parameters orderby entry.Key descending select entry;

                    string val = value;
                    foreach (var s in sortedDict)
                    {
                        val = val.Remove(s.Key) + "{s" + s.Value + "}" + val.Substring(s.Key + s.Value.ToString().Length + 2);
                    }

                    currentFolder.codeLines.Add($"public static string {keyVariable}({parameterStr}) => string.Format(Dict[\"{key}\"], {parameterPass});");
                    //output += $"public string {key}({parameterStr}) => $\"{val}\";\n" + tabs;
                }
            }

            return root.Print(TAB);
        }

        public static string Generate(string nameSpace, params string[] locaFiles)
        {
            string locaCode = "";

            foreach (var file in locaFiles)
            {
                locaCode += GenerateFromFile(file);
            }

            locaCode =
$@"using System.Collections.Generic;
using System.IO;

namespace {nameSpace}
{{{locaCode}
}}";
            return locaCode;
        }
    }
}
