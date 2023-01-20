
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BytingLib
{
    public class Cylinder3 : IShape3
    {
        private Vector3 pos;
        public Vector3 Length;
        public float Radius;

        public Cylinder3(Vector3 pos, Vector3 length, float radius)
        {
            this.pos = pos;
            Length = length;
            Radius = radius;
        }

        public Vector3 Pos { get => pos; set => pos = value; }
        public float X { get => pos.X; set => pos.X = value; }
        public float Y { get => pos.Y; set => pos.Y = value; }
        public float Z { get => pos.Z; set => pos.Z = value; }
        public Vector3 Pos2 => Pos + Length;
        public Vector3 Center => pos + Length * 0.5f;

        public Type GetCollisionType() => typeof(Cylinder3);

        public virtual object Clone() => MemberwiseClone();

        public BoundingBox GetBoundingBox()
        {
            Vector3 a = Length;
            Vector3 e = Radius * Vector3Extension.GetSqrt(Vector3.One - a * a / Vector3.Dot(a, a));
            return new BoundingBox(Vector3.Min(Pos - e, Pos + Length - e)
                , Vector3.Max(Pos + e, Pos + Length + e));
        }

        public void Render(PrimitiveBatcherOld batcher, Color color)
        {
            var b = batcher.TriBatcher;

            // the cylinder has <FACES> faces on the side -> <FACES>*2 vertices <FACES>*6 indices
            // and two disk faces on the caps -> <FACES>*2 vertices and <FACES>*2 indices
            const int FACES = 12;
            const int WallVertices = FACES * 2;
            const int CapVerticesTotal = FACES * 2;
            const int IndicesPerCap = 3 * (FACES - 2);
            b.EnsureAdditionalArrayCapacity(WallVertices + CapVerticesTotal, FACES * 6 + IndicesPerCap * 2);

            // indices
            // ...
            // 4 - 5
            // | / |
            // 2 - 3
            // | / |
            // 0 - 1
            int currentIndex = b.verticesIndex;
            for (int i = 0; i < FACES; i++)
            {
                b.indices[b.indicesIndex++] = currentIndex + 0;
                b.indices[b.indicesIndex++] = currentIndex + 3;
                b.indices[b.indicesIndex++] = currentIndex + 1;
                b.indices[b.indicesIndex++] = currentIndex + 0;
                b.indices[b.indicesIndex++] = currentIndex + 2;
                b.indices[b.indicesIndex++] = currentIndex + 3;
                currentIndex += 2;
            }
            // last two vertices are the first two. So let the last indices point to them
            b.indices[b.indicesIndex - 5] = b.verticesIndex + 1;
            b.indices[b.indicesIndex - 2] = b.verticesIndex + 0;
            b.indices[b.indicesIndex - 1] = b.verticesIndex + 1;

            // indices for caps
            // 1 - 2 - 3 - 4 - 5
            // | /   /   /   /
            // 0 - - - - - - 
            for (int cap = 0; cap < 2; cap++)
            {
                int centerIndex = currentIndex;
                for (int i = 1; i < FACES; i++)
                {
                    b.indices[b.indicesIndex++] = centerIndex;
                    b.indices[b.indicesIndex++] = currentIndex;
                    b.indices[b.indicesIndex++] = ++currentIndex;
                }
                currentIndex++;
            }

            // cylinder walls and caps
            Vector3 xAxis = Vector3.Normalize(Vector3.Cross(Length, Vector3Extension.GetNonParallelVector(Length)));
            Vector3 yAxis = Vector3.Normalize(Vector3.Cross(Length, xAxis));
            float toAngle = -MathHelper.TwoPi / FACES;
            Vector3 capNormal1 = Vector3.Normalize(Length);
            Vector3 capNormal2 = -capNormal1;

            int cap1VerticesIndex = b.verticesIndex + WallVertices;
            int cap2VerticesIndex = cap1VerticesIndex + CapVerticesTotal;

            for (int i = 0; i < FACES; i++)
            {
                float angle = i * toAngle;
                Vector3 n = MathF.Cos(angle) * xAxis + MathF.Sin(angle) * yAxis;
                Vector3 p1 = pos + n * Radius;
                Vector3 p2 = pos + Length + n * Radius;
                // cylinder walls
                b.vertices[b.verticesIndex++] = new(p1, color, n);
                b.vertices[b.verticesIndex++] = new(p2, color, n);

                // cylinder caps
                b.vertices[cap1VerticesIndex++] = new(p2, color, capNormal1);
                b.vertices[--cap2VerticesIndex] = new(p1, color, capNormal2);
            }

            b.verticesIndex += CapVerticesTotal;



        }
    }
}
