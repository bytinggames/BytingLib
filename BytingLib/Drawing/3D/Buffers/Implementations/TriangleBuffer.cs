using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BytingLib
{
    class TriangleBuffer : RenderBuffer<VertexPositionNormal>, IRenderBuffer<Triangle3>
    {
        public TriangleBuffer(GraphicsDevice gDevice)
            : base(gDevice, new VertexPositionNormal[]
                {  new(Vector3.Zero, Vector3.UnitZ), new(Vector3.UnitY, Vector3.UnitZ), new(Vector3.UnitX, Vector3.UnitZ) },
                new short[] { 0, 1, 2 }
                )
        {
        }

        public override bool HasNormal => true;

        public void Add(Triangle3 tri, Color color)
        {
            Add(ToTransform(tri), color);
        }

        private Matrix ToTransform(Triangle3 tri)
        {
            return new Matrix(new Vector4(tri.DirB, 0),
                            new Vector4(tri.DirA, 0),
                            new Vector4(tri.N, 0),
                            new Vector4(tri.Pos, 1));
        }
    }
}
