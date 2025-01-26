namespace BytingLib.Markup
{
    /// <summary>Elements that are rendered.</summary>
    public interface ILeaf : INode
    {
        void Draw(MarkupSettings settings);
        Vector2 GetSize(MarkupSettings settings, int startIndex = 0, int endIndex = -1);
        /// <summary>Wether the maximum height of this element is interpreted as the line spacing of the font.</summary>
        bool ConfinesToLineSpacing { get; }
    }
}
