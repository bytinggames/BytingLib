namespace BytingLib
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class CreatorShortcutAttribute : Attribute
    {
        public readonly string ShortcutName;

        public CreatorShortcutAttribute(string shortcutName)
        {
            this.ShortcutName = shortcutName;
        }
    }
}
