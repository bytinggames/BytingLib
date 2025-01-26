using System.Text.RegularExpressions;

namespace BytingLib.Markup
{
    [MarkupShortcut("after_sentence")]
    class MarkupAfterSentence : MarkupCollection
    {
        public MarkupAfterSentence(Creator creator, string text, string append)
            : base(creator, AppendAfterSentence(text, append))
        {
        }

        static string AppendAfterSentence(string text, string append)
        {
            return Regex.Replace(text, "[;.!?！。]", f => f + append);
        }
    }
}
