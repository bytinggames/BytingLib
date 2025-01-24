namespace BytingLib.Markup
{
    [MarkupShortcut("nobreak")]
    public class MarkupNoBreak : MarkupCollection
    {
        public MarkupNoBreak(Creator creator, string text)
            : base(creator, text)
        {
        }
    }
}
