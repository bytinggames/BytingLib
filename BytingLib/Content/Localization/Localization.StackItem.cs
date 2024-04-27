namespace BytingLib
{
    public partial class Localization
    {
        class StackItem
        {
            public int lineIndex;
            public string localKey;
            public int childIndex = 0;

            public StackItem(int lineIndex, string key)
            {
                this.lineIndex = lineIndex;
                this.localKey = key;
            }

            public override string ToString()
            {
                return localKey + " " + childIndex;
            }
        }
    }
}
