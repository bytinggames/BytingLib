
namespace BytingLib
{
    public class ScriptReaderException : Exception
    {
        public ScriptReaderException(string message) : base(message) { }
    }

    public class ScriptReader
    {
        private string str;
        private int i;

        public ScriptReader(string str)
        {
            this.str = str;
            i = 0;
        }

        public char? ReadChar()
        {
            if (i < str.Length)
                return str[i++];
            return null;
        }

        public string ReadToCharOrEnd(out char? foundChar, params char[] chars)
        {
            int start = i;
            char? c;
            while ((c = ReadChar()) != null)
            {
                if (chars.Contains(c.Value))
                {
                    int end = i - 1;
                    foundChar = str[end];
                    return str.Substring(start, end - start);
                }
            }
            foundChar = null;
            return str.Substring(start);
        }

        public void Move(int moveBy)
        {
            i += moveBy;
            if (i < 0)
                i = 0;
            if (i > str.Length)
                i = str.Length;
        }

        public string ReadToChar(char untilChar)
        {
            int start = i;
            char? c;
            while ((c = ReadChar()) != null)
            {
                if (c.Value == untilChar)
                {
                    int end = i - 1;
                    return str.Substring(start, end - start);
                }
            }
            throw new ScriptReaderException("char '" + untilChar + "' not found in " + str + " after position " + start);
        }

        public string ReadToChar(out char? foundChar, params char[] chars)
        {
            int start = i;
            string str = ReadToCharOrEnd(out foundChar, chars);
            if (foundChar == null)
                throw new ScriptReaderException("chars '" + string.Join(", ", chars) + "' not found in " + str + " after position " + start);
            return str;
        }

        public string ReadUntilClosed(char open, char close, int openCounter = 1)
        {
            int start = i;
            char? c;
            while ((c = ReadChar()) != null)
            {
                if (c.Value == open)
                {
                    openCounter++;
                }
                else if (c.Value == close)
                {
                    openCounter--;
                    if (openCounter <= 0)
                    {
                        int end = i - 1;
                        return str.Substring(start, end - start);
                    }
                }
            }
            throw new ScriptReaderException(open + " " + close + " " + openCounter + " " + str + " " + i);
        }

        public char? Peek() => i < str.Length ? (char?)str[i] : null;
        public char? Peek(int relative)
        {
            relative += i;
            return relative >= 0 && relative < str.Length ? (char?)str[relative] : null;
        }
        public void Insert(string v)
        {
            str = str.Insert(i, v);
        }

        public string ReadToCharOrEndConsiderOpenCloseBraces(char untilChar, char open, char close)
        {
            int start = i;
            char? c;
            while ((c = ReadChar()) != null)
            {
                if (c.Value == untilChar)
                {
                    int end = i - 1;
                    return str.Substring(start, end - start);
                }
                else if (c.Value == open)
                {
                    ReadUntilClosed(open, close);
                }
            }
            return str.Substring(start);
        }

        public string ReadToCharOrEndConsiderOpenCloseBraces(char[] untilChar, char open, char close)
        {
            int start = i;
            char? c;
            while ((c = ReadChar()) != null)
            {
                if (untilChar.Contains(c.Value))
                {
                    int end = i - 1;
                    return str.Substring(start, end - start);
                }
                else if (c.Value == open)
                {
                    ReadUntilClosed(open, close);
                }
            }
            return str.Substring(start);
        }

        public bool EndOfString() => i >= str.Length;

        public override string ToString()
        {
            char? c = Peek();
            if (c == null)
                return "END";
            return $"{i}: '{c.Value}'";
        }
    }
}
