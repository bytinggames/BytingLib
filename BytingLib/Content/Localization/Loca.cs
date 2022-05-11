using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BytingLib
{
    public static class Loca
    {
        public static CultureInfo Culture { get; private set; }

        private const char separator = ';';
        private const char textMarker = '"';
        private const char adder = '.';
        private const char nestedLevel = '\t';

        private static Dictionary<string, string> dictionary = new Dictionary<string, string>();

        public static string LanguageKey { get; private set; }

        static string GetDisplayName()
        {
            //var displayName = Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName;
            return "en"; // currently only en is supported
        }

        static Loca()
        {
            Initialize(GetDisplayName(), File.ReadAllLines(Path.Combine("Content", "localization.csv")));
        }

        public static void Reload(string fromPath)
        {
            Initialize(GetDisplayName(), File.ReadAllLines(fromPath));
        }

        class StackItem
        {
            public string key;
            public int subKeyCount;

            public StackItem(string key)
            {
                this.key = key;
                this.subKeyCount = 0;
            }

            public override string ToString()
            {
                return key + " " + subKeyCount;
            }
        }

        private static void Initialize(string languageKey, string[] localizationLines)
        {
            if (dictionary != null)
                dictionary.Clear();

            dictionary.Add("", ""); // add empty

            if (localizationLines.Length == 0)
                return;

            string head = localizationLines[0];
            int languageIndex = -1;
            int nextI = 0;
            bool foundLanguage = false;
            for (int i = 0; nextI < head.Length; i = nextI + 1)
            {
                nextI = head.IndexOf(separator, i);

                if (nextI == -1)
                    nextI = head.Length;

                languageIndex++;
                string language = head.Substring(i, nextI - i);
                if (language == languageKey)
                {
                    foundLanguage = true;
                    break;
                }
            }

            if (!foundLanguage)
                throw new InvalidDataException("language " + languageKey + " not found");

            List<StackItem> keyStack = new List<StackItem>();
            keyStack.Add(new StackItem("NONE"));

            string key, value;
            for (int i = 1; i < localizationLines.Length; i++)
            {
                int indexStart = -1;
                int indexEnd = -1;

                indexStart = indexEnd + 1;

                bool textMarked;

                indexEnd = GetCell(indexEnd, out textMarked);

                key = GetCellValue(indexStart, indexEnd, textMarked);

                for (int j = 0; j < languageIndex; j++)
                {
                    indexStart = indexEnd + 1;

                    indexEnd = GetCell(indexEnd, out textMarked);
                }

                value = GetCellValue(indexStart, indexEnd, textMarked);

                int newStackSize;
                for (newStackSize = 0; newStackSize < key.Length && key[newStackSize] == nestedLevel; newStackSize++) ;
                key = key.Substring(newStackSize);
                newStackSize++;

                if (key == "")
                    continue;

                if (value == "")
                    throw new InvalidDataException("Translation missing for key \"" + key + "\" (localization.csv)");

                if (newStackSize == keyStack.Count + 1)
                {
                    keyStack.Add(new StackItem(key));
                }
                else if (newStackSize == keyStack.Count)
                {
                    keyStack[keyStack.Count - 1] = new StackItem(key);
                }
                else
                {
                    // remove keyStack until equal to minusCount
                    while (keyStack.Count > newStackSize)
                    {
                        keyStack.RemoveAt(keyStack.Count - 1);
                    }
                    keyStack[keyStack.Count - 1] = new StackItem(key);
                }

                if (keyStack.Count > 1)
                {
                    keyStack[keyStack.Count - 2].subKeyCount++;

                    if (key == "#") // replace # with count
                    {
                        keyStack[keyStack.Count - 1].key = (keyStack[keyStack.Count - 2].subKeyCount - 1).ToString();
                    }
                }

                key = "";
                for (int j = 0; j < keyStack.Count; j++)
                {
                    if (j > 0)
                        key += adder;
                    key += keyStack[j].key;
                }

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
                        }
                        while (openBlocksCount > 0);
                        string command = value.Substring(jStart + 1, j - jStart - 1);
                        if (command.Length > 0)
                        {
                            string replacement = null;
                            if (command == "+")
                            {
                                // use same as language 1 (en)
                                replacement = GetFirstLanguageCellValue();
                            }
                            else if (command[0] < '0' || command[0] > '9')
                            {
                                // loca key
                                if (command[0] == '.')
                                {
                                    // relative key
                                    string currentKey = key.Remove(key.Length - keyStack[keyStack.Count - 1].key.Length);
                                    currentKey += command.Substring(1);
                                    replacement = InnerE(currentKey);
                                }
                                else if (command[0] == ':')
                                {
                                    // relative upwards key
                                    string currentKey = key;
                                    int k;
                                    currentKey = currentKey.Remove(currentKey.Length - keyStack[keyStack.Count - 1].key.Length);
                                    for (k = 1; k < command.Length && command[k - 1] == ':'; k++)
                                    {
                                        currentKey = currentKey.Remove(currentKey.Length - keyStack[keyStack.Count - 1 - k].key.Length - 1); // - 1 because of the dot
                                    }

                                    currentKey += command.Substring(k - 1);

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
                                        return "";
                                    if (c[c.Length - 1] != '}')
                                        return Localize(c);

                                    int searchIndex = 0;
                                    List<string> parameters = new List<string>();

                                    string realKey = null;

                                    while ((searchIndex = c.IndexOf('{', searchIndex)) != -1)
                                    {
                                        if (realKey == null)
                                            realKey = c.Remove(searchIndex);

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
                                        }
                                        while (openBlocksCount > 0);

                                        // params inside command detected
                                        parameters.Add(c.Substring(searchIndex, end - searchIndex));
                                        searchIndex = end + 1;
                                    }

                                    return Get(realKey, parameters.ToArray());
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

                if (dictionary.ContainsKey(key))
                    throw new InvalidDataException($"The key {key} is defined two or more times in the Localization.csv file.");

                dictionary.Add(key, value);

                int GetCell(int index, out bool marked)
                {
                    index++; // jump over semicolon
                    if (localizationLines[i].Length > index && localizationLines[i][index] == textMarker)
                    {
                        marked = true;
                        // find marker ending
                        index++; // jump over first marker
                        while (true)
                        {
                            if (localizationLines[i][index] == textMarker)
                            {
                                index++;
                                if (localizationLines[i].Length == index || localizationLines[i][index] != textMarker)
                                {
                                    break;
                                }
                            }

                            index++;
                            if (index >= localizationLines[i].Length)
                                throw new InvalidDataException("End of " + textMarker + " marker not found");
                        }

                        if (localizationLines[i].Length < index && localizationLines[i][index] != separator)
                            throw new InvalidDataException("after " + textMarker + " marker, no separator " + separator + " found");

                        //return (true, KeyedByTypeCollection);
                    }
                    else
                    {
                        marked = false;
                        index = localizationLines[i].IndexOf(separator, index);
                    }

                    return index;
                }

                string GetCellValue(int start, int end, bool marked)
                {
                    if (end == -1)
                        end = localizationLines[i].Length;
                    if (marked)
                        return localizationLines[i].Substring(start + 1, end - start - 2).Replace("\"\"", "\""); // minus the 2 markers
                    else
                        return localizationLines[i].Substring(start, end - start);
                }

                string GetFirstLanguageCellValue()
                {
                    int start = localizationLines[i].IndexOf(separator);
                    int end = GetCell(start, out bool marked);
                    return GetCellValue(start + 1, end, marked);
                }
            }

        }

        public static string Get(string key, params object[] args)
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
                    throw new Exception("key params were opened with '{' but were not closed with '}')");
            }

            string value = Localize(key);
            if (args == null || args.Length == 0)
                return value;
            value = string.Format(value, args);
            return value;
        }

        private static string Localize(string key)
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

        public static bool Contains(string key)
        {
            return dictionary.ContainsKey(key);
        }
    }
}
