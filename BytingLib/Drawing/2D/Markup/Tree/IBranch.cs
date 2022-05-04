using System.Collections.Generic;

namespace BytingLib.Markup
{
    /// <summary>Elements that contain other elements. May modify the draw settings.</summary>
    public interface IBranch : INode
    {
        IEnumerable<ILeaf> IterateOverLeaves(MarkupSettings settings);
    }
}
