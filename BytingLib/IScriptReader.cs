namespace BytingLib
{
    public interface IScriptReader
    {
        bool EndOfString();
        void Insert(string v);
        void Move(int moveBy);
        char? Peek();
        char? Peek(int relative);
        char? ReadChar();
        string ReadToChar(char untilChar);
        string ReadToChar(out char? foundChar, params char[] chars);
        string ReadToCharOrEnd(out char? foundChar, params char[] chars);
        string ReadToCharOrEndConsiderOpenCloseBraces(char untilChar, char open, char close);
        string ReadToCharOrEndConsiderOpenCloseBraces(char[] untilChar, char open, char close);
        string ReadUntilClosed(char open, char close, int openCounter = 1);
    }
}