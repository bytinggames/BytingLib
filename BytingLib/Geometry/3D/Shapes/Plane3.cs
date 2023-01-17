
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BytingLib
{
    public class Plane3 : IShape3
    {
        private Vector3 pos;
        public Vector3 Normal;

        /// <summary>Be sure to pass a normalized nomral.</summary>
        public Plane3(Vector3 pos, Vector3 normal)
        {
            Pos = pos;
            Normal = normal;
        }

        public Vector3 Pos { get => pos; set => pos = value; }
        public float X { get => pos.X; set => pos.X = value; }
        public float Y { get => pos.Y; set => pos.Y = value; }
        public float Z { get => pos.Z; set => pos.Z = value; }

        public Type GetCollisionType() => typeof(Plane3);

        public virtual object Clone() => MemberwiseClone();

        public BoundingBox GetBoundingBox()
        {
            throw new NotImplementedException();
        }

        public void Render(PrimitiveBatcher batcher, Color color)
        {
            Render(batcher, color, 1000f);
        }

        public void Render(PrimitiveBatcher batcher, Color color, float size)
        {
            var b = batcher.TriBatcher;

            b.EnsureAdditionalArrayCapacity(4, 6);

            // indices
            // 3 - 2
            // | / |
            // 0 - 1
            b.indices[b.indicesIndex++] = b.verticesIndex + 0;
            b.indices[b.indicesIndex++] = b.verticesIndex + 2;
            b.indices[b.indicesIndex++] = b.verticesIndex + 1;
            b.indices[b.indicesIndex++] = b.verticesIndex + 0;
            b.indices[b.indicesIndex++] = b.verticesIndex + 3;
            b.indices[b.indicesIndex++] = b.verticesIndex + 2;

            Vector3 X = Vector3.Normalize(Vector3.Cross(Normal, Vector3Extension.GetNonParallelVector(Normal))) * size / 2f;
            Vector3 Y = Vector3.Normalize(Vector3.Cross(Normal, X)) * size / 2f;

            b.vertices[b.verticesIndex++] = new(pos - X - Y, color, Normal);
            b.vertices[b.verticesIndex++] = new(pos + X - Y, color, Normal);
            b.vertices[b.verticesIndex++] = new(pos + X + Y, color, Normal);
            b.vertices[b.verticesIndex++] = new(pos - X + Y, color, Normal);

        }
    }
}
