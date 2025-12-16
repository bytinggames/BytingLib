namespace BytingLib.Markup
{
    /// <summary>
    /// Use Markup.Escape() which will output something like #escape(8|ign#or|e)
    /// </summary>
    [MarkupShortcut("escape")]
    class MarkupEscape : MarkupText
    {
        public MarkupEscape(string text, IStringEdit? edit) : base(text, edit)
        {
        }
    }
}
