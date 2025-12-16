namespace BytingLib
{
    public class StringEditCollection : IStringEdit
    {
        public IList<IStringEdit> Edits { get; }

        public StringEditCollection(IList<IStringEdit> edits)
        {
            Edits = edits;
        }
        public StringEditCollection(params IStringEdit[] edits)
        {
            Edits = edits;
        }

        public string GetApplied(string str)
        {
            foreach (var edit in Edits)
            {
                str = edit.GetApplied(str);
            }
            return str;
        }
    }
}