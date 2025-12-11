using System.Text.RegularExpressions;

namespace BytingLib.Markup
{
    [MarkupShortcut("after_sentence")]
    public class MarkupAfterSentence : MarkupCollection
    {
        public MarkupAfterSentence(Creator creator, string text, string append)
            : base(creator, AppendAfterSentence(text, append))
        {
        }

        public MarkupAfterSentence(Creator creator, string text, string append, string extraSentenceEndingCharacters)
            : base(creator, AppendAfterSentence(text, append, extraSentenceEndingCharacters))
        {
        }

        public static string AppendAfterSentence(string text, string append, string extraSentenceEndingCharacters = "")
        {
            return Regex.Replace(text, "[;.!?！。．？‼⁇⁈⁉"+ extraSentenceEndingCharacters + "]", f => f + append);
        }
    }
}
