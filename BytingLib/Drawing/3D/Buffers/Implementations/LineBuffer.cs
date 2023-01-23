using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BytingLib
{
    class LineBuffer : RenderBuffer<VertexPosition>, IRenderBuffer<Line3>
    {
        public LineBuffer(GraphicsDevice gDevice)
            : base(gDevice, new VertexPosition[]
                {  new(Vector3.Zero), new(Vector3.UnitX) },
                new short[] { 0, 1 }
                )
        {
        }

        public override bool HasNormal => false;

        public void Add(Line3 line, Color color)
        {
            Add(ToTransform(line), color);
        }

        private Matrix ToTransform(Line3 line)
        {
            return new Matrix(new Vector4(line.Dir, 0),
                            Vector4.Zero,
                            Vector4.Zero,
                            new Vector4(line.Pos, 1));
        }
    }
}
