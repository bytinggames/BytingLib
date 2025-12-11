namespace BytingLib.Markup
{
    [MarkupShortcut("after_sentence_or_comma")]
    public class MarkupAfterSentenceOrComma : MarkupAfterSentence
    {
        const string commas = ",，";

        public MarkupAfterSentenceOrComma(Creator creator, string text, string append = "") : base(creator, text, append + commas)
        {
        }

        public static new string AppendAfterSentence(string text, string append, string extraSentenceEndingCharacters = "")
        {
            return MarkupAfterSentence.AppendAfterSentence(text, append, commas);
        }
    }
}
