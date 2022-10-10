using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace BytingLib
{
    public interface IShape3Collection : IShape3
    {
        public IEnumerable<IShape3> ShapesEnumerable { get; }
    }
}
