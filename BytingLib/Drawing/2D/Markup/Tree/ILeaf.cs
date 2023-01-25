namespace BytingLib.Markup
{
    /// <summary>Elements that are rendered.</summary>
    public interface ILeaf : INode
    {
        void Draw(MarkupSettings settings);
        Vector2 GetSize(MarkupSettings settings);
    }
}
