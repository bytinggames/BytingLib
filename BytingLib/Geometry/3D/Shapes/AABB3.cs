
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BytingLib
{
    public class AABB3 : IShape3
    {
        private Vector3 pos;
        public Vector3 Size;

        public AABB3(Vector3 pos, Vector3 size)
        {
            Pos = pos;
            Size = size;
        }

        public static AABB3 FromCenter(Vector3 centerPos, Vector3 size)
        {
            return new AABB3(centerPos - size / 2f, size);
        }

        public virtual Vector3 Pos { get => pos; set => pos = value; }
        public virtual float X { get => pos.X; set => pos.X = value; }
        public virtual float Y { get => pos.Y; set => pos.Y = value; }
        public virtual float Z { get => pos.Z; set => pos.Z = value; }
        public virtual Vector3 Min => pos;
        public virtual Vector3 Max => pos + Size;
        public Vector3 Center
        {
            get => Pos + Size / 2f;
            set => Pos = value - Size / 2f;
        }

        public Type GetCollisionType() => typeof(AABB3);

        public virtual object Clone()
        {
            return new AABB3(pos, Size);
        }

        public BoundingBox GetBoundingBox()
        {
            return new BoundingBox(Min, Max);
        }

        public Vector3 MoveVectorInside(Vector3 pos)
        {
            Vector3 nearestInBox;
            if (pos.X < Min.X)
                nearestInBox.X = Min.X;
            else if (pos.X > Max.X)
                nearestInBox.X = Max.X;
            else
                nearestInBox.X = pos.X;

            if (pos.Y < Min.Y)
                nearestInBox.Y = Min.Y;
            else if (pos.Y > Max.Y)
                nearestInBox.Y = Max.Y;
            else
                nearestInBox.Y = pos.Y;

            if (pos.Z < Min.Z)
                nearestInBox.Z = Min.Z;
            else if (pos.Z > Max.Z)
                nearestInBox.Z = Max.Z;
            else
                nearestInBox.Z = pos.Z;
            return nearestInBox;
        }

        public Box3 ToBox()
        {
            return new Box3(Matrix.CreateScale(Size) * Matrix.CreateTranslation(Center));
        }

        public Vector3 GetPos(Vector3 normalizedPosInside)
        {
            return Min + (Max - Min) * normalizedPosInside;
        }

        public void Render(PrimitiveBatcherOld batcher, Color color)
        {
            var b = batcher.TriBatcher;

            const int faces = 6;
            // draw 6 quads, where each quad has 6 indices and 4 vertices
            b.EnsureAdditionalArrayCapacity(4 * faces, 6 * faces);

            // indices
            // 3 - 2
            // | / |
            // 0 - 1
            int startIndex = b.verticesIndex;
            for (int face = 0; face < faces; face++)
            {
                b.indices[b.indicesIndex++] = startIndex + 0;
                b.indices[b.indicesIndex++] = startIndex + 2;
                b.indices[b.indicesIndex++] = startIndex + 1;
                b.indices[b.indicesIndex++] = startIndex + 0;
                b.indices[b.indicesIndex++] = startIndex + 3;
                b.indices[b.indicesIndex++] = startIndex + 2;
                startIndex += 4;
            }

            // +z face
            b.vertices[b.verticesIndex++] = new(new Vector3(Min.X, Min.Y, Max.Z), color, Vector3.UnitZ);
            b.vertices[b.verticesIndex++] = new(new Vector3(Max.X, Min.Y, Max.Z), color, Vector3.UnitZ);
            b.vertices[b.verticesIndex++] = new(new Vector3(Max.X, Max.Y, Max.Z), color, Vector3.UnitZ);
            b.vertices[b.verticesIndex++] = new(new Vector3(Min.X, Max.Y, Max.Z), color, Vector3.UnitZ);

            // +y face
            b.vertices[b.verticesIndex++] = new(new Vector3(Min.X, Max.Y, Max.Z), color, Vector3.UnitY);
            b.vertices[b.verticesIndex++] = new(new Vector3(Max.X, Max.Y, Max.Z), color, Vector3.UnitY);
            b.vertices[b.verticesIndex++] = new(new Vector3(Max.X, Max.Y, Min.Z), color, Vector3.UnitY);
            b.vertices[b.verticesIndex++] = new(new Vector3(Min.X, Max.Y, Min.Z), color, Vector3.UnitY);

            // -z face
            b.vertices[b.verticesIndex++] = new(new Vector3(Min.X, Max.Y, Min.Z), color, -Vector3.UnitZ);
            b.vertices[b.verticesIndex++] = new(new Vector3(Max.X, Max.Y, Min.Z), color, -Vector3.UnitZ);
            b.vertices[b.verticesIndex++] = new(new Vector3(Max.X, Min.Y, Min.Z), color, -Vector3.UnitZ);
            b.vertices[b.verticesIndex++] = new(new Vector3(Min.X, Min.Y, Min.Z), color, -Vector3.UnitZ);

            // -y face
            b.vertices[b.verticesIndex++] = new(new Vector3(Min.X, Min.Y, Min.Z), color, -Vector3.UnitY);
            b.vertices[b.verticesIndex++] = new(new Vector3(Max.X, Min.Y, Min.Z), color, -Vector3.UnitY);
            b.vertices[b.verticesIndex++] = new(new Vector3(Max.X, Min.Y, Max.Z), color, -Vector3.UnitY);
            b.vertices[b.verticesIndex++] = new(new Vector3(Min.X, Min.Y, Max.Z), color, -Vector3.UnitY);

            // +x face
            b.vertices[b.verticesIndex++] = new(new Vector3(Max.X, Min.Y, Max.Z), color, Vector3.UnitX);
            b.vertices[b.verticesIndex++] = new(new Vector3(Max.X, Min.Y, Min.Z), color, Vector3.UnitX);
            b.vertices[b.verticesIndex++] = new(new Vector3(Max.X, Max.Y, Min.Z), color, Vector3.UnitX);
            b.vertices[b.verticesIndex++] = new(new Vector3(Max.X, Max.Y, Max.Z), color, Vector3.UnitX);

            // -x face
            b.vertices[b.verticesIndex++] = new(new Vector3(Min.X, Min.Y, Min.Z), color, -Vector3.UnitX);
            b.vertices[b.verticesIndex++] = new(new Vector3(Min.X, Min.Y, Max.Z), color, -Vector3.UnitX);
            b.vertices[b.verticesIndex++] = new(new Vector3(Min.X, Max.Y, Max.Z), color, -Vector3.UnitX);
            b.vertices[b.verticesIndex++] = new(new Vector3(Min.X, Max.Y, Min.Z), color, -Vector3.UnitX);

        }
    }
}
