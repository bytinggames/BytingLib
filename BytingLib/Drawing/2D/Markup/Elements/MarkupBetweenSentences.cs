using System.Text.RegularExpressions;

namespace BytingLib.Markup
{
    [MarkupShortcut("between_sentences")]
    public class MarkupBetweenSentences : MarkupCollection
    {
        public MarkupBetweenSentences(Creator creator, string text, string append)
            : base(creator, AppendAfterSentence(text, append))
        {
        }

        public MarkupBetweenSentences(Creator creator, string text, string append, string extraSentenceEndingCharacters)
            : base(creator, AppendAfterSentence(text, append, extraSentenceEndingCharacters))
        {
        }

        public static string AppendAfterSentence(string text, string append, string extraSentenceEndingCharacters = "")
        {
            return Regex.Replace(text.Remove(text.Length - 1), "[;.!?！。．？‼⁇⁈⁉"+ extraSentenceEndingCharacters + "]", f => f + append) + text[^1];
        }
    }
}
