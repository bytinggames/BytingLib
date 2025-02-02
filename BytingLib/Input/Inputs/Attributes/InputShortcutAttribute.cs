namespace BytingLib
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class InputShortcutAttribute : CreatorShortcutAttribute
    {
        public InputShortcutAttribute(string shortcutName)
            : base(shortcutName)
        {
        }
    }
}
