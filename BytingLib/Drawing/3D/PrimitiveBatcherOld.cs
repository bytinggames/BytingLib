
using Microsoft.Xna.Framework.Graphics;

namespace BytingLib
{
    public class PrimitiveBatcherOld
    {
        public PrimitiveBatcherOld(MeshBatcher<VertexPositionColorNormal> triBatcher, MeshBatcher<VertexPositionColor> lineBatcher)
        {
            TriBatcher = triBatcher;
            LineBatcher = lineBatcher;
        }

        public MeshBatcher<VertexPositionColorNormal> TriBatcher { get; set; }
        public MeshBatcher<VertexPositionColor> LineBatcher { get; set; }
    }
}
