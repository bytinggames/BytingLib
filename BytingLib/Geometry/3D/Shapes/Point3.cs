
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BytingLib
{
    public class Point3 : IShape3
    {
        private Vector3 pos;

        public Point3(Vector3 pos)
        {
            this.pos = pos;
        }

        public Vector3 Pos { get => pos; set => pos = value; }
        public float X { get => pos.X; set => pos.X = value; }
        public float Y { get => pos.Y; set => pos.Y = value; }
        public float Z { get => pos.Z; set => pos.Z = value; }

        public Type GetCollisionType() => typeof(Point3);

        public virtual object Clone() => MemberwiseClone();

        public BoundingBox GetBoundingBox()
        {
            return new BoundingBox(Pos, Pos);
        }

        public void Render(PrimitiveBatcherOld batcher, Color color)
        {
            Render(batcher, color, 1f);
        }

        public void RenderCross(PrimitiveBatcherOld batcher, Color color)
        {
            RenderCross(batcher, color, 3);
        }

        public void RenderCross(PrimitiveBatcherOld batcher, Color color, float lineLength)
        {
            float l = MathF.Pow(lineLength / 2f, 1f / 3f);
            // draw a cross
            var b = batcher.LineBatcher;

            b.EnsureAdditionalArrayCapacity(8, 8);

            // indices
            for (int i = 0; i < 8; i++)
                b.indices[b.indicesIndex++] = b.verticesIndex + i;


            b.vertices[b.verticesIndex++] = new(Pos + new Vector3(l, l, l), color);
            b.vertices[b.verticesIndex++] = new(Pos - new Vector3(l, l, l), color);

            b.vertices[b.verticesIndex++] = new(Pos + new Vector3(-l, l, l), color);
            b.vertices[b.verticesIndex++] = new(Pos - new Vector3(-l, l, l), color);

            b.vertices[b.verticesIndex++] = new(Pos + new Vector3(l, -l, l), color);
            b.vertices[b.verticesIndex++] = new(Pos - new Vector3(l, -l, l), color);

            b.vertices[b.verticesIndex++] = new(Pos + new Vector3(l, l, -l), color);
            b.vertices[b.verticesIndex++] = new(Pos - new Vector3(l, l, -l), color);
        }

        public void Render(PrimitiveBatcherOld batcher, Color color, float radius)
        {
            new Sphere3(pos, radius).RenderSimple(batcher, color);
        }
    }
}
