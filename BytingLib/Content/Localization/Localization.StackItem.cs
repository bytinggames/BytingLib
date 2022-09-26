namespace BytingLib
{
    public partial class Localization
    {
        class StackItem
        {
            public string key;
            public int subKeyCount;

            public StackItem(string key)
            {
                this.key = key;
                this.subKeyCount = 0;
            }

            public override string ToString()
            {
                return key + " " + subKeyCount;
            }
        }
    }
}
