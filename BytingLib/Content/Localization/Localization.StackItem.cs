namespace BytingLib
{
    public partial class Localization
    {
        class StackItem(int lineIndex, string key, bool isIntendedToBeTranslated)
        {
            public int LineIndex = lineIndex;
            public string LocalKey = key;
            public int NumberedIndex = 0;
            public bool IsIntendedToBeTranslated = isIntendedToBeTranslated;

            public override string ToString()
            {
                return LocalKey + " " + NumberedIndex;
            }
        }
    }
}
