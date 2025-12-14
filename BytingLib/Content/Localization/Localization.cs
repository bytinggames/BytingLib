using System.Text;
using System.Text.RegularExpressions;

namespace BytingLib
{
    public partial class Localization : ILocaChanger
    {
        private readonly char separator;
        private const char textMarker = '"';
        private const char levelSeparator = '_';
        private const char tagOpen = '<';
        private const char tagClose = '>';
        private const char tagSlash = '/';
        private const char nestedLevel = '\t';
        private const char parameterSplit = '§';
        private const char literalCharacter = '@';
        private const char plus = '+';
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
            bool skipPluses = false, Localization? locaOverride = null, char separator = ';')
        {
            this.csvFile = csvFile;
            this.separator = separator;
            LanguageKey = languageKey;
            this.defaultLanguage = defaultLanguage;
            this.fallbackToFirstLanguage = fallbackToFirstLanguage;
            this.resolveValues = resolveValues;
            this.skipPluses = skipPluses;
            this.locaOverride = locaOverride;
            Initialize();
        }

        private static string[] CsvFileToLines(string file)
        {
            string[] lines = File.ReadAllLines(file, Encoding.UTF8);

            // could be moved to a content processor
            ReplaceParameterTags(lines);
            ReplacePlussesWithTags(lines);

            return lines;
        }

        private static void ReplaceParameterTags(string[] lines)
        {
            for (int i = 0; i < lines.Length; i++)
            {
                lines[i] = ReplaceNamedArgsWithNumbers(lines[i]);
            }
        }

        static string ReplaceNamedArgsWithNumbers(string text)
        {
            var map = new Dictionary<string, int>();

            return Regex.Replace(text, @"\{(\w+)\}", m =>
            {
                var key = m.Groups[1].Value;

                if (!map.TryGetValue(key, out int index))
                {
                    if (key.Length > 0 && char.IsDigit(key[0]))
                    {
                        return "{" + key + "}";
                    }

                    index = map.Count;
                    map[key] = index;
                }

                return "{" + index + "}";
            });
        }

        private static void ReplacePlussesWithTags(string[] lines)
        {
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].EndsWith($";{plus}"))
                {
                    lines[i] = lines[i].Remove(lines[i].Length - 1) + $"<{plus}/>";
                }
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
            InitializeInner();

            TriggerReloadSubs();
        }

        private void InitializeInner()
        {
            string[] localizationLines = CsvFileToLines(csvFile);

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

            int languageColumn = GetLanguageColumn(out defaultLanguageIndex, out bool unknownLanguage);

            if (locaOverride != null && unknownLanguage)
            {
                int separatorCounts = localizationLines[0].Count(f => f == ';');
                int addSeparators = languageColumn - separatorCounts;

                localizationLines[0] += new string(separator, addSeparators) + LanguageKey;
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
                        keyDirectory += levelSeparator;
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
                bool endsWithPlus = localizationLines[lineIndex].EndsWith($";<{plus}/>");
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
                        throw new Exception($"{keyDirectory}.{localKey} is missing {(fallbackToFirstLanguage ? "any" : "a")} translation at line {lineIndex + 1}.\nIf this key isn't intended to be translated, make sure the line ends with '<+/>'.");
                    }
                }

                string key = keyDirectory;
                if (key != "")
                {
                    key += levelSeparator;
                }
                key += localKey;

                if (resolveValues)
                {
                    ParseRawLocaString(ref value);

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


                void ParseRawLocaString(ref string value)
                {
                    // could be made more efficient

                    // value commands <_some_key>, <some_key>, <Some_key> and <+/>
                    ScriptReaderLiteral reader = new ScriptReaderLiteral(value, literalCharacter);
                    while (true)
                    {
                        string str = reader.ReadToCharOrEnd(out char? found, out bool omittedCharacters, tagOpen);
                        if (omittedCharacters)
                        {
                            value = str;
                        }
                        if (found == null)
                        {
                            break;
                        }

                        int i = reader.Position - 1;
                        int beforeTag = i;
                        (string tag, string[]? args) = ParseTagRecursively(ref value, ref i);
                        ReplaceTag(ref value, beforeTag, ref i, tag, args);
                        reader = new(value, literalCharacter);
                        reader.SetPosition(i + 1);
                    }
                }

                void ReplaceTag(ref string line, int beforeWholeTag, ref int afterWholeTag, string tag, string[]? args)
                {
                    if (tag.Length > 0)
                    {
                        string? replacement = null;
                        if (tag.Length == 1 && tag[0] == plus)
                        {
                            // use same as language defaultLanguageIndex (en)
                            replacement = GetCell(lineIndex, defaultLanguageIndex, localizationLines);
                            if (replacement != null)
                            {
                                ParseRawLocaString(ref replacement);
                            }
                        }
                        else
                        {
                            // loca key
                            if (tag[0] == levelSeparator)
                            {
                                // relative upwards key
                                string currentKey = keyDirectory;
                                int k;
                                for (k = 2; k < tag.Length && tag[k - 1] == levelSeparator; k++)
                                {
                                    currentKey = currentKey.Remove(currentKey.LastIndexOf(levelSeparator));
                                }

                                if (currentKey != "")
                                {
                                    currentKey += levelSeparator;
                                }

                                currentKey += tag.Substring(k - 1);

                                replacement = InnerE(currentKey, args);
                            }
                            else if (char.IsLower(tag[0]))
                            {
                                // relative downwards key (equal to .currentNode.)
                                string currentKey = key + levelSeparator + tag;
                                replacement = InnerE(currentKey, args);
                            }
                            else
                            {
                                // command is upper case
                                // absolute key
                                replacement = InnerE(tag, args);
                            }

                            string InnerE(string c, string[]? args)
                            {
                                if (c.Length == 0)
                                {
                                    return "";
                                }

                                if (args == null)
                                {
                                    return Localize(c);
                                }
                                return Get(c, args);
                            }
                        }

                        if (replacement != null)
                        {
                            line = line.Remove(beforeWholeTag) + replacement + line.Substring(afterWholeTag);
                            afterWholeTag = beforeWholeTag + replacement.Length - 1;
                        }
                    }
                }




                (string tag, string[]? parameters) ParseTagRecursively(ref string line, ref int i)
                {
                    i++; // go over <
                    int tagCloseIndex = line.IndexOf(tagClose, i);
                    if (tagCloseIndex == -1)
                    {
                        throw new Exception("tag wasn't closed");
                    }
                    string tag = line.Substring(i, tagCloseIndex - i);
                    i = tagCloseIndex + 1; // go over >
                    if (tag.EndsWith(tagSlash))
                    {
                        // simple <tag/>
                        tag = tag.Remove(tag.Length - 1);
                        return (tag, null);
                    }
                    int parametersStart = i;
                    do
                    {
                        if (line[i] == tagOpen)
                        {
                            if (i + 1 >= line.Length)
                            {
                                throw new Exception("< tag wasn't closed with >");
                            }
                            bool secondTag = line[i + 1] == tagSlash;

                            if (secondTag)
                            {
                                int parametersEnd = i; // before </
                                i = line.IndexOf(tagClose, i + 1);
                                if (i == -1)
                                {
                                    throw new Exception("</ wasn't closed");
                                }
                                i++;
                                string parameters = line.Substring(parametersStart, parametersEnd - parametersStart);
                                return (tag, parameters.Split(parameterSplit));
                            }
                            else
                            {
                                int beforeTag = i;
                                (string innerTag, string[]? innerParameters) = ParseTagRecursively(ref line, ref i);
                                ReplaceTag(ref line, beforeTag, ref i, innerTag, innerParameters);
                            }
                        }
                        i++;
                    } while (i < line.Length);

                    throw new Exception("didn't close all openend brackets: " + line);
                }
            }

            int GetLanguageColumn(out int defaultLanguageColumn, out bool unknownLanguage)
            {
                int i = 0;
                int languageColumn = -1;
                defaultLanguageColumn = -1;
                unknownLanguage = false;
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
                            if (fallbackToFirstLanguage)
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
                                // create new column
                                languageColumn = i;
                                unknownLanguage = true;
                                break;
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
            if (value.StartsWith(textMarker))
            {
                if (!value.EndsWith(textMarker))
                {
                    throw new Exception("cell doesn't end with \"");
                }

                if (!value.Contains(separator))
                {
                    value = value.Substring(1, value.Length - 2).Replace(textMarker.ToString() + textMarker.ToString(), textMarker.ToString());
                }
            }

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
                if (localizationLines[lineIndex].EndsWith($"<{plus}/>"))
                {
                    return $"<{plus}/>";
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
                            // end reached. this is the last column
                            if (column > 0)
                            {
                                return null;
                            }
                            index = localizationLines[lineIndex].Length;
                        }
                        else
                        {
                            throw new Exception("end of \" escaped string not found in " + localizationLines[lineIndex]);
                        }
                    }
                    else
                    {
                        index++; // skip over "
                    }
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

        public string Get(string key, params object[]? args)
        {
            //int braceOpenIndex = key.IndexOf(parameterOpen);
            //if (braceOpenIndex != -1)
            //{
            //    int braceCloseIndex = key.IndexOf(parameterClose);
            //    if (braceCloseIndex != -1)
            //    {
            //        string parameters = key.Substring(braceOpenIndex + 1, braceCloseIndex - braceOpenIndex - 1);
            //        key = key.Remove(braceOpenIndex, braceCloseIndex + 1 - braceOpenIndex);
            //        args = parameters.Split(new char[] { ',' }).Concat(args).ToArray();
            //    }
            //    else
            //    {
            //        throw new Exception($"key params were opened with '{parameterOpen}' but were not closed with '{parameterClose}')");
            //    }
            //}

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
            if (char.IsLower(key[0]))
            {
                throw new Exception($"key {key} must start with upper case letter");
            }

            //if (char.IsUpper(key[0]))
            //{
            //    key = key[0].ToString().ToLower() + key.Substring(1);
            //}

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
            Localization?[] locas = new Localization?[columns.Length];

            for (int i = 0; i < locas.Length; i++)
            {
                if (DoesLanguageExist(locaFile, columns[i]))
                {
                    locas[i] = new(locaFile, columns[i], defaultLanguageKey, false, false, true);
                }
            }

            string[] keys = locas.FirstOrDefault(f => f != null)!.dictionary.Keys.ToArray();

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
                    csv += separator;
                    if (locas[j] != null)
                    {
                        csv += locas[j]!.dictionary[key];
                    }
                }
            }

            return csv;
        }

        private static bool DoesLanguageExist(string locaFile, string language, char separator = ';')
        {
            return File.ReadLines(locaFile).First().Split([separator]).Any(f => f == language);
        }

        public static void CsvImportFromTranslator(string locaFile, string translatorFile, char separator, string languageKey, string defaultLanguageKey = "en")
        {
            // either:
            // put back into tabbed csv
            //      + probably fastest method to implement right now
            //      + instantly visible if translation is missing
            //      + don't need to sync
            //      - big file, could be messy to look at or edit
            // keep translated csvs separated?
            //      +-? seperation keeps it more organized

            Localization translated = new(translatorFile, languageKey, defaultLanguageKey, false, false, true, null, separator);

            Localization loca = new(locaFile, languageKey, defaultLanguageKey, false, false, true, translated, ';');

            if (loca.CsvOutput != null)
            {
                File.WriteAllLines(locaFile, loca.CsvOutput);
            }
        }

    }
}
