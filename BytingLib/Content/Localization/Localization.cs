using System.Text;

namespace BytingLib
{
    public partial class Localization : ILocaChanger
    {
        private const char separator = ';';
        private const char textMarker = '"';
        private const char adder = '.';
        private const char nestedLevel = '\t';
        private string csvFile;
        private readonly bool fallbackToFirstLanguage;
        private Dictionary<string, string> dictionary = new Dictionary<string, string>();

        public event Action? OnLocaReload;

        public string LanguageKey { get; private set; }

        public Localization(string csvFile, string languageKey, bool fallbackToFirstLanguage = true)
        {
            this.csvFile = csvFile;
            LanguageKey = languageKey;
            this.fallbackToFirstLanguage = fallbackToFirstLanguage;
            Initialize();
        }

        private string[] CsvFileToLines(string file)
        {
            return File.ReadAllLines(file, Encoding.UTF8); // file.Replace("\r", "").Split(new char[] { '\n' });
        }

        public void Reload()
        {
            Initialize();
        }

        /// <summary>This does also reload if the languageKey was already the same.</summary>
        public void Reload(string languageKey)
        {
            LanguageKey = languageKey;
            Reload();
        }

        public void Reload(string csvFile, string languageKey)
        {
            this.csvFile = csvFile;
            LanguageKey = languageKey;
            Reload();
        }

        private void Initialize()
        {
            InitializeInner();

            TriggerReloadSubs();
        }

        private void InitializeInner()
        {
            string[] localizationLines = CsvFileToLines(csvFile);

            if (dictionary != null)
            {
                dictionary.Clear();
            }
            else
            {
                dictionary = new Dictionary<string, string>();
            }

            dictionary.Add("", ""); // add empty

            if (localizationLines.Length == 0)
            {
                return;
            }

            int languageColumn = 0;
            while (true)
            {
                languageColumn++; // start at column 1
                string? lan = GetCell(0, languageColumn); // languages reside in column 0
                if (lan == null)
                {
                    throw new Exception($"language {LanguageKey} not found");
                }
                if (lan == LanguageKey)
                {
                    break;
                }
            }

            Stack<StackItem> stack = new();
            stack.Push(new StackItem(-1, "PLACEHOLDER", false));
            string keyDirectory = "";
            for (int i = 1; i < localizationLines.Length; i++)
            {
                int lineIndentation = GetIndentation(localizationLines[i]);
                bool isParent = false;
                if (i + 1 < localizationLines.Length)
                {
                    if (GetIndentation(localizationLines[i + 1]) > lineIndentation)
                    {
                        isParent = true;
                    }
                }

                if (lineIndentation < stack.Count - 1) // going back?
                {
                    ParseParentsUntilIndentation(lineIndentation);
                }

                bool isIntendedToBeTranslated;

                if (lineIndentation <= stack.Count - 1) // same level or going back?
                {
                    StackItem top = stack.Peek();
                    top.ChildIndex++;
                    string localKey = GetKey(localizationLines[i], lineIndentation, top.ChildIndex, out isIntendedToBeTranslated);
                    top.IsIntendedToBeTranslated = isIntendedToBeTranslated;
                    top.LocalKey = localKey;
                    top.LineIndex = i;
                }
                else if (lineIndentation == stack.Count) // one level deeper?
                {
                    // add key of top element from stack to keyDirectory
                    if (keyDirectory.Length > 0)
                    {
                        keyDirectory += adder;
                    }
                    keyDirectory += stack.Peek().LocalKey;

                    // add parent
                    string localKey = GetKey(localizationLines[i], lineIndentation, 0, out isIntendedToBeTranslated);
                    stack.Push(new StackItem(i, localKey, isIntendedToBeTranslated));
                }
                else // going too deep?
                {
                    throw new Exception("Indentation cannot exceed the previous line by more than one tab: " + localizationLines[i]);
                }
                // parse the current line (if not a parent and if it's even intended to be translated (line contains ';'))
                if (!isParent && isIntendedToBeTranslated)
                {
                    ParseLine(i, keyDirectory, stack.Peek().LocalKey);
                }
            }

            // don't forget to parse the last parents
            ParseParentsUntilIndentation(0);

            void ParseParentsUntilIndentation(int lineIndentation)
            {
                while (stack.Count - 1 > lineIndentation)
                {
                    stack.Pop();
                    StackItem item = stack.Peek();
                    int removeAdder = stack.Count > 1 ? -1 : 0;
                    keyDirectory = keyDirectory.Remove(keyDirectory.Length - item.LocalKey.Length + removeAdder);
                    if (item.IsIntendedToBeTranslated)
                    {
                        // parse parent now
                        ParseLine(item.LineIndex, keyDirectory, item.LocalKey);
                    }
                }
            }

            void ParseLine(int lineIndex, string keyDirectory, string localKey)
            {
                string? value = GetCell(lineIndex, languageColumn);

                if (string.IsNullOrEmpty(value))
                {
                    // fall back to first language (if not already first language)
                    if (fallbackToFirstLanguage
                        && languageColumn != 1)
                    {
                        value = GetCell(lineIndex, 1);
                    }

                    if (string.IsNullOrEmpty(value))
                    {
                        // no translation whatsoever. not even fallback english
                        throw new Exception($"{keyDirectory}.{localKey} is missing {(fallbackToFirstLanguage ? "any" : "a")} translation at line {lineIndex + 1}.\nIf this key isn't intended to be translated, make sure there is no ';' in that line.");
                    }
                }

                string key = keyDirectory;
                if (key != "")
                {
                    key += ".";
                }
                key += localKey;

                #region value commands {+}, {:} and {.}

                for (int j = 0; j < value.Length; j++)
                {
                    if (value[j] == '{')
                    {
                        int jStart = j;
                        int openBlocksCount = 1;
                        do
                        {
                            j++;
                            switch (value[j])
                            {
                                case '}': openBlocksCount--; break;
                                case '{': openBlocksCount++; break;
                            }
                        } while (openBlocksCount > 0 && j + 1 < value.Length);

                        if (openBlocksCount > 0)
                        {
                            throw new Exception("didn't close all openend brackets: " + value);
                        }

                        string command = value.Substring(jStart + 1, j - jStart - 1);
                        if (command.Length > 0)
                        {
                            string? replacement = null;
                            if (command == "+")
                            {
                                // use same as language 1 (en)
                                replacement = GetCell(lineIndex, 1);
                            }
                            else if (command[0] < '0' || command[0] > '9')
                            {
                                // loca key
                                if (command[0] == '.')
                                {
                                    // relative key
                                    string currentKey = keyDirectory;
                                    if (currentKey != "")
                                    {
                                        currentKey += ".";
                                    }
                                    currentKey += command.Substring(1);
                                    replacement = InnerE(currentKey);
                                }
                                else if (command[0] == ':')
                                {
                                    // relative upwards key
                                    string currentKey = keyDirectory;
                                    int k;
                                    for (k = 1; k < command.Length && command[k - 1] == ':'; k++)
                                    {
                                        currentKey = currentKey.Remove(currentKey.LastIndexOf('.'));
                                    }

                                    if (currentKey != "")
                                    {
                                        currentKey += ".";
                                    }

                                    currentKey += command.Substring(k - 1);

                                    replacement = InnerE(currentKey);
                                }
                                else if (command[0] == '>')
                                {
                                    // relative downwards key (equal to .currentNode.)
                                    string currentKey = key;
                                    currentKey += "." + command.Substring(1);
                                    replacement = InnerE(currentKey);
                                }
                                else
                                {
                                    // absolute key
                                    replacement = InnerE(command);
                                }

                                string InnerE(string c)
                                {
                                    if (c.Length == 0)
                                    {
                                        return "";
                                    }

                                    if (c[c.Length - 1] != '}')
                                    {
                                        return Localize(c);
                                    }

                                    int searchIndex = 0;
                                    List<string> parameters = new List<string>();

                                    string? realKey = null;

                                    while ((searchIndex = c.IndexOf('{', searchIndex)) != -1)
                                    {
                                        if (realKey == null)
                                        {
                                            realKey = c.Remove(searchIndex);
                                        }

                                        searchIndex++;


                                        int openBlocksCount = 1;
                                        int end = searchIndex - 1;
                                        do
                                        {
                                            end++;
                                            switch (c[end])
                                            {
                                                case '}': openBlocksCount--; break;
                                                case '{': openBlocksCount++; break;
                                            }
                                        } while (openBlocksCount > 0 && end + 1 < c.Length);

                                        if (openBlocksCount > 0)
                                        {
                                            throw new Exception("didn't close all openend brackets: " + c);
                                        }

                                        // params inside command detected
                                        parameters.Add(c.Substring(searchIndex, end - searchIndex));
                                        searchIndex = end + 1;
                                    }

                                    return Get(realKey!, parameters.ToArray());
                                }
                            }

                            if (replacement != null)
                            {
                                value = value.Remove(jStart) + replacement + value.Substring(j + 1);
                                j = jStart - 1;
                            }
                        }
                    }
                }

                #endregion

                value = value.Replace("\\n", "\n");

                dictionary.Add(key, value);
            }
            string? GetCell(int lineIndex, int column)
            {
                int index = -1;
                int previousIndex = -1;
                while (column >= 0)
                {
                    previousIndex = index;

                    // is this cell embedded in "?
                    bool embeddedInQuotes = localizationLines[lineIndex][index + 1] == textMarker;

                    if (embeddedInQuotes)
                    {
                        index++; // skip over "
                        index = localizationLines[lineIndex].IndexOf(textMarker.ToString() + separator, index + 1);
                        index++; // skip over "
                    }
                    else
                    {
                        index = localizationLines[lineIndex].IndexOf(separator, index + 1);
                    }

                    if (index == -1)
                    {
                        // end reached. this is the last column
                        if (column > 0)
                        {
                            return null;
                        }
                        index = localizationLines[lineIndex].Length;
                    }

                    column--;
                }

                previousIndex++; // go over separator

                // trim textMarker?
                if (localizationLines[lineIndex].Length > previousIndex
                    && localizationLines[lineIndex][previousIndex] == textMarker)
                {
                    if (localizationLines[lineIndex][index - 1] == textMarker)
                    {
                        // trim textMarker
                        previousIndex++;
                        index--;

                        return localizationLines[lineIndex].Substring(previousIndex, index - previousIndex)
                            .Replace("\"\"", "\"");
                    }
                    else
                    {
                        throw new InvalidDataException("End of " + textMarker + " marker not found in line " + (lineIndex + 1));
                    }
                }
                return localizationLines[lineIndex].Substring(previousIndex, index - previousIndex);
            }
        }

        public string Get(string key, params object[] args)
        {
            int braceOpenIndex = key.IndexOf('{');
            if (braceOpenIndex != -1)
            {
                int braceCloseIndex = key.IndexOf('}');
                if (braceCloseIndex != -1)
                {
                    string parameters = key.Substring(braceOpenIndex + 1, braceCloseIndex - braceOpenIndex - 1);
                    key = key.Remove(braceOpenIndex, braceCloseIndex + 1 - braceOpenIndex);
                    args = parameters.Split(new char[] { ',' }).Concat(args).ToArray();
                }
                else
                {
                    throw new Exception("key params were opened with '{' but were not closed with '}')");
                }
            }

            string value = Localize(key);
            if (args == null || args.Length == 0)
            {
                return value;
            }

            value = string.Format(value, args);
            return value;
        }

        private string Localize(string key)
        {
            if (!dictionary.ContainsKey(key))
            {
                Exception e = new Exception("key not found: " + key);
#if DEBUG
                throw e;
#else
                //Logger.Log(e);
                return "[" + key + "]";
#endif
            }

            if (dictionary[key] == "")
            {
                Exception e = new Exception("not yet translated: " + key);
#if DEBUG
                throw e;
#else
                //Logger.Log(e);
                return "[" + key + "]";
#endif
            }
            return dictionary[key];
        }

        public bool Contains(string key)
        {
            return dictionary.ContainsKey(key);
        }

        private void TriggerReloadSubs()
        {
            OnLocaReload?.Invoke();
        }

        private int GetIndentation(string line)
        {
            int i;
            for (i = 0; i < line.Length; i++)
            {
                if (line[i] != nestedLevel)
                {
                    return i;
                }
            }
            return i;
        }

        private string GetKey(string line, int indentation, int childIndex, out bool isIntendedToBeTranslated)
        {
            int separatorIndex = line.IndexOf(separator, indentation);
            if (separatorIndex != -1)
            {
                isIntendedToBeTranslated = true;
                line = line.Substring(indentation, separatorIndex - indentation);
            }
            else
            {
                isIntendedToBeTranslated = false;
                line = line.Substring(indentation);
            }
            if (line == "#") // replace # with child index
            {
                line = childIndex.ToString();
            }
            return line;
        }

        public Dictionary<string, string> GetDictionary() => dictionary;
    }
}
