using System.Text;

namespace BytingLib
{
    public partial class Localization : ILocaChanger
    {
        private readonly char separator;
        private readonly bool convertStars;
        private const char textMarker = '"';
        private const char adder = '.';
        private const char nestedLevel = '\t';
        private string csvFile;
        private readonly string defaultLanguage;
        private readonly bool fallbackToFirstLanguage;
        private readonly bool resolveValues;
        private readonly bool skipPluses;
        private readonly Localization? locaOverride;
        private Dictionary<string, string> dictionary = new Dictionary<string, string>();
        private int defaultLanguageIndex;

        public event Action? OnLocaReload;

        public string LanguageKey { get; private set; }
        public string[]? CsvOutput { get; private set; }


        public Localization(string csvFile, string languageKey, string defaultLanguage = "en", bool fallbackToFirstLanguage = true, bool resolveValues = true, 
            bool skipPluses = false, Localization? locaOverride = null, char separator = ';', bool convertStars = true)
        {
            this.csvFile = csvFile;
            this.separator = separator;
            this.convertStars = convertStars;
            LanguageKey = languageKey;
            this.defaultLanguage = defaultLanguage;
            this.fallbackToFirstLanguage = fallbackToFirstLanguage;
            this.resolveValues = resolveValues;
            this.skipPluses = skipPluses;
            this.locaOverride = locaOverride;
            Initialize();
        }

        private static string[] CsvFileToLines(string file, bool convertStars)
        {
            string[] lines = File.ReadAllLines(file, Encoding.UTF8);

            if (convertStars)
            {
                for (int i = 0; i < lines.Length; i++)
                {
                    bool anyStars = false;
                    for (int j = 0; j < lines[i].Length; j++)
                    {
                        if (lines[i][j] == '*')
                        {
                            anyStars = true;
                            break;
                        }
                    }
                    if (anyStars)
                    {
                        ReplaceStars(ref lines[i]);
                    }
                }
            }

            return lines;
        }

        private static void ReplaceStars(ref string str)
        {
            StringBuilder builder = new(str);
            char? c;
            int i = 0;
            while ((c = ReadChar()) != null )
            {
                if (c == '{')
                {
                    c = ReadChar();
                    if (c == '*')
                    {
                        // open
                        int bracketStart = i - 2;
                        string idStr = "";
                        while ((c = ReadChar()) != null)
                        {
                            if (c == '}')
                            {
                                break;
                            }
                            idStr += c;
                        }
                        if (c == null)
                        {
                            throw new Exception("unexpected end");
                        }
                        int id;
                        if (idStr == "")
                        {
                            id = 1;
                        }
                        else
                        {
                            id = int.Parse(idStr);
                        }

                        builder.Remove(bracketStart + 1, 2 + idStr.Length); //  remove *2}      { remains
                        string newStr = $">{id}{{";
                        int insertInto = bracketStart + 1;
                        builder.Insert(insertInto, newStr); //        insert >2{      {>2{ remains
                        i = insertInto + newStr.Length;
                    }
                    else if (c == '/' && ReadChar() == '*' && ReadChar() == '}')
                    {
                        // close
                        int bracketStart = i - 4;
                        builder.Remove(bracketStart, 3); //     remove {/*      } remains
                        builder.Insert(bracketStart, '}'); //   insert }        }} remains
                        i = bracketStart + 2;
                    }
                }
            }

            str = builder.ToString();

            char? ReadChar()
            {
                if (i >= builder.Length)
                {
                    return null;
                }
                return builder[i++];
            }
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
            InitializeInner(convertStars);

            TriggerReloadSubs();
        }

        private void InitializeInner(bool convertStars)
        {
            string[] localizationLines = CsvFileToLines(csvFile, convertStars);

            if (locaOverride != null)
            {
                CsvOutput = localizationLines;
            }

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

            int languageColumn = GetLanguageColumn(out defaultLanguageIndex);

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
                if ((!isParent || !resolveValues) // only parse children, except we don't resolve values, then also parse parents
                    && isIntendedToBeTranslated)
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
                        if (resolveValues) // if resolving values, we parse parents directly, when iterating them, not after having iterated over all of their children
                        {
                            ParseLine(item.LineIndex, keyDirectory, item.LocalKey);
                        }
                    }
                }
            }

            void ParseLine(int lineIndex, string keyDirectory, string localKey)
            {
                bool endsWithPlus = localizationLines[lineIndex].EndsWith("{+}");
                if (skipPluses && endsWithPlus)
                {
                    return;
                }

                string? value = GetCell(lineIndex, languageColumn, localizationLines);

                if (string.IsNullOrEmpty(value))
                {
                    // fall back to first language (if not already first language)
                    if (fallbackToFirstLanguage
                        && languageColumn != defaultLanguageIndex)
                    {
                        value = GetCell(lineIndex, defaultLanguageIndex, localizationLines);
                    }

                    if (!resolveValues)
                    {
                        value = ""; // if not given, simply use "" to refer to not translated yet
                    }
                    else if (string.IsNullOrEmpty(value))
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

                if (resolveValues)
                {
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
                                    // use same as language defaultLanguageIndex (en)
                                    replacement = GetCell(lineIndex, defaultLanguageIndex, localizationLines);
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
                }
                else
                {
                    value = GetCell(lineIndex, languageColumn, localizationLines);

                    if (value == null)
                    {
                        value = "";
                    }
                }
                dictionary.Add(key, value);

                if (locaOverride != null)
                {
                    if (locaOverride.dictionary.TryGetValue(key, out string? val))
                    {
                        locaOverride.dictionary.Remove(key);
                        if (val != null)
                        {
                            SetCell(lineIndex, languageColumn, localizationLines, val);
                        }
                    }
                }
            }

            int GetLanguageColumn(out int defaultLanguageColumn)
            {
                int i = 0;
                int languageColumn = -1;
                defaultLanguageColumn = -1;
                while (true)
                {
                    i++; // start at column 1
                    string? lan = GetCell(0, i, localizationLines); // languages reside in column 0

                    if (lan == defaultLanguage)
                    {
                        defaultLanguageColumn = i;
                    }

                    if (lan == null)
                    {
                        // last column reached
                        if (languageColumn == -1)
                        {
                            if (LanguageKey != defaultLanguage && defaultLanguage != null)
                            {
                                // language {LanguageKey} not found
                                // fallback to default language
                                LanguageKey = defaultLanguage;
                                languageColumn = defaultLanguageColumn;
                                if (defaultLanguageColumn == -1)
                                {
                                    throw new Exception("defaultLanguageColumn == -1 shouldn't happen");
                                }
                                break;
                            }
                            else
                            {
                                throw new Exception($"language {LanguageKey} not found and no fallback language provided");
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                    if (lan == LanguageKey)
                    {
                        languageColumn = i;
                    }
                }
                return languageColumn;
            }
        }

        void SetCell(int lineIndex, int column, string[] localizationLines, string value)
        {
            var indices = GetCellIndices(lineIndex, ref column, localizationLines);
            if (indices == null)
            {
                while (column > 0)
                {
                    localizationLines[lineIndex] += separator;
                    column--;
                }
                localizationLines[lineIndex] += value;

                return;
            }
            localizationLines[lineIndex] = localizationLines[lineIndex].Remove(indices.Value.previousIndex)
                + value
                + localizationLines[lineIndex].Substring(indices.Value.index);
        }
        string? GetCell(int lineIndex, int column, string[] localizationLines)
        {
            var indices = GetCellIndices(lineIndex, ref column, localizationLines);
            if (indices == null)
            {
                if (localizationLines[lineIndex].EndsWith("{+}"))
                {
                    return "{+}";
                }

                return null;
            }

            int previousIndex = indices.Value.previousIndex;
            int index = indices.Value.index;

            // trim textMarker?
            if (resolveValues)
            {
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
            }

            return localizationLines[lineIndex].Substring(indices.Value.previousIndex, index - previousIndex);
        }

        private (int index, int previousIndex)? GetCellIndices(int lineIndex, ref int column, string[] localizationLines)
        {
            int index = -1;
            int previousIndex = -1;
            while (column >= 0)
            {
                previousIndex = index;

                // is this cell embedded in "?
                if (index + 1 >= localizationLines[lineIndex].Length)
                {
                    return null;
                }
                bool embeddedInQuotes = localizationLines[lineIndex][index + 1] == textMarker;

                if (embeddedInQuotes)
                {
                    index++; // skip over "
                    index = localizationLines[lineIndex].IndexOf(textMarker.ToString() + separator, index + 1);
                    if (index == -1)
                    {
                        if (localizationLines[lineIndex][^1] == textMarker)
                        {
                            index = localizationLines[lineIndex].Length - 1;
                        }
                    }
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

            return (index, previousIndex);
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

        private static int GetIndentation(string line)
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

        record CsvExportRow(string SourceValue, string TargetValue, string Comment);

        public static string CsvExportForTranslator(string locaFile, string[] columns, string defaultLanguageKey = "en", char separator = ';')
        {
            Localization[] locas = new Localization[columns.Length];

            for (int i = 0; i < locas.Length; i++)
            {
                locas[i] = new(locaFile, columns[i], defaultLanguageKey, false, false, true, null, ';', false);
            }

            string[] keys = locas[0].dictionary.Keys.ToArray();

            // first row (csv head / columns)
            string csv = "key";
            for (int i = 0; i < columns.Length; i++)
            {
                csv += separator + columns[i];
            }

            // rows
            for (int i = 1; i < keys.Length; i++)
            {
                csv += "\r\n";

                var key = keys[i];
                csv += key;
                for (int j = 0; j < locas.Length; j++)
                {
                    csv += separator + locas[j].dictionary[key];
                }
            }

            return csv;
        }

        public static void CsvImportFromTranslator(string locaFile, string translatorFile, char separator, int targetLanguageColumnIndex = 2, string defaultLanguageKey = "en")
        {
            // either:
            // put back into tabbed csv
            //      + probably fastest method to implement right now
            //      + instantly visible if translation is missing
            //      + don't need to sync
            //      - big file, could be messy to look at or edit
            // keep translated csvs separated?
            //      +-? seperation keeps it more organized


            string[] translatedLines = File.ReadAllLines(translatorFile, Encoding.UTF8);
            string[] columns = translatedLines[0].Split([separator]);
            string targetLanguage = columns[targetLanguageColumnIndex];

            Localization translated = new(translatorFile, targetLanguage, defaultLanguageKey, false, false, true, null, separator, false);

            Localization loca = new(locaFile, targetLanguage, defaultLanguageKey, false, false, true, translated, ';', false);

            if (loca.CsvOutput != null)
            {
                File.WriteAllLines(locaFile, loca.CsvOutput);
            }
        }

    }
}
