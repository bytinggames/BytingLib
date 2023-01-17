
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BytingLib
{
    public class AxisRadius3 : IShape3
    {
        private Vector3 pos;
        /// <summary>This vector should never be set to a non-normalized vector.</summary>
        public Vector3 Dir;
        public float Radius;

        /// <summary>Be sure to pass a normalized direction.</summary>
        public AxisRadius3(Vector3 position, Vector3 normalizedDirection, float radius)
        {
            pos = position;
            Dir = normalizedDirection;
            Radius = radius;
        }

        public Vector3 Pos { get => pos; set => pos = value; }
        public float X { get => pos.X; set => pos.X = value; }
        public float Y { get => pos.Y; set => pos.Y = value; }
        public float Z { get => pos.Z; set => pos.Z = value; }

        public Type GetCollisionType() => typeof(AxisRadius3);

        public virtual object Clone() => MemberwiseClone();

        public BoundingBox GetBoundingBox()
        {
            Vector3 boundingSize = Vector3.Zero;

            if (Dir.X != 0)
                boundingSize.X = float.PositiveInfinity;
            else
                boundingSize.X = Radius;
            if (Dir.Y != 0)
                boundingSize.Y = float.PositiveInfinity;
            else
                boundingSize.Y = Radius;
            if (Dir.Z != 0)
                boundingSize.Z = float.PositiveInfinity;
            else
                boundingSize.Z = Radius;

            return new BoundingBox(Pos - boundingSize, Pos + boundingSize);
        }

        public void Render(PrimitiveBatcher batcher, Color color)
        {
            Render(batcher, color, -1000f, 1000f);
        }

        public void Render(PrimitiveBatcher batcher, Color color, float cylinderMin, float cylinderMax)
        {
            var b = batcher.TriBatcher;

            // draw cylinder without caps
            // the cylinder has <FACES> faces -> <FACES>*2 vertices <FACES>*6 indices
            const int FACES = 12;
            b.EnsureAdditionalArrayCapacity(FACES * 2, FACES * 6);

            // indices
            // ...
            // 4 - 5
            // | / |
            // 2 - 3
            // | / |
            // 0 - 1
            int startIndex = b.verticesIndex;
            for (int i = 0; i < FACES; i++)
            {
                b.indices[b.indicesIndex++] = startIndex + 0;
                b.indices[b.indicesIndex++] = startIndex + 3;
                b.indices[b.indicesIndex++] = startIndex + 1;
                b.indices[b.indicesIndex++] = startIndex + 0;
                b.indices[b.indicesIndex++] = startIndex + 2;
                b.indices[b.indicesIndex++] = startIndex + 3;
                startIndex += 2;
            }
            // last two vertices are the first two. So let the last indices point to them
            b.indices[b.indicesIndex - 5] = b.verticesIndex + 1;
            b.indices[b.indicesIndex - 2] = b.verticesIndex + 0;
            b.indices[b.indicesIndex - 1] = b.verticesIndex + 1;

            // line: start - pos - end
            Vector3 xAxis = Vector3.Normalize(Vector3.Cross(Dir, Vector3Extension.GetNonParallelVector(Dir)));
            Vector3 yAxis = Vector3.Normalize(Vector3.Cross(Dir, xAxis));
            float toAngle = -MathHelper.TwoPi / FACES;
            Vector3 p1 = pos + Dir * cylinderMin;
            Vector3 p2 = pos + Dir * cylinderMax;
            for (int i = 0; i < FACES; i++)
            {
                float angle = i * toAngle;
                Vector3 n = MathF.Cos(angle) * xAxis + MathF.Sin(angle) * yAxis;
                b.vertices[b.verticesIndex++] = new(p1 + n * Radius, color, n);
                b.vertices[b.verticesIndex++] = new(p2 + n * Radius, color, n);
            }
        }
    }
}
