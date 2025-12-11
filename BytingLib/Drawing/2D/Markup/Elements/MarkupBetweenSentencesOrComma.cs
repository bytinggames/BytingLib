namespace BytingLib.Markup
{
    [MarkupShortcut("between_sentences_or_comma")]
    public class MarkupBetweenSentencesOrComma : MarkupAfterSentence
    {
        const string commas = ",，";

        public MarkupBetweenSentencesOrComma(Creator creator, string text, string append = "") : base(creator, text, append + commas)
        {
        }

        public static new string AppendAfterSentence(string text, string append, string extraSentenceEndingCharacters = "")
        {
            return MarkupBetweenSentences.AppendAfterSentence(text, append, commas);
        }
    }
}
