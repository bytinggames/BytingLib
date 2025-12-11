namespace BytingLib.Markup
{
    public class MarkupNewLine : ILeaf
    {
        public bool ConfinesToLineSpacing => true;

        public void Draw(MarkupSettings settings) { }

        public Vector2 GetSize(MarkupSettings settings, int start, int end)
        {
            return new Vector2(0, MathF.Max(settings.MinLineHeight, settings.LineSpacing));
        }

        public override string ToString()
        {
            return "\\n";
        }

        public void Dispose()
        {
        }
    }
}
