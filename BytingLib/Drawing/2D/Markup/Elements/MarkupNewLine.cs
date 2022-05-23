using Microsoft.Xna.Framework;

namespace BytingLib.Markup
{
    public class MarkupNewLine : ILeaf
    {
        public void Draw(MarkupSettings settings) { }

        public Vector2 GetSize(MarkupSettings settings)
        {
            return new Vector2(0, settings.Font.Value.LineSpacing * settings.Scale.Y);
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
