using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BytingLib
{
    public class Sphere3 : IShape3
    {
        private Vector3 pos;
        public float Radius;

        public Sphere3(Vector3 position, float radius)
        {
            pos = position;
            Radius = radius;
        }

        public Vector3 Pos { get => pos; set => pos = value; }
        public float X { get => pos.X; set => pos.X = value; }
        public float Y { get => pos.Y; set => pos.Y = value; }
        public float Z { get => pos.Z; set => pos.Z = value; }

        public Type GetCollisionType() => typeof(Sphere3);

        public virtual object Clone() => MemberwiseClone();

        public BoundingBox GetBoundingBox()
        {
            Vector3 r = new Vector3(Radius);
            return new BoundingBox(Pos - r, Pos + r);
        }

        //public void Render(PrimitiveBatcherOld batcher, Color color)
        //{
        //    var b = batcher.TriBatcher;

        //    var v = Icosahedron.VerticesSub;
        //    var ind = Icosahedron.IndicesSub;

        //    b.EnsureAdditionalArrayCapacity(v.Length, ind.Length * 3);

        //    for (int i = 0; i < ind.Length; i++)
        //    {
        //        b.indices[b.indicesIndex++] = b.verticesIndex + ind[i][0];
        //        b.indices[b.indicesIndex++] = b.verticesIndex + ind[i][1];
        //        b.indices[b.indicesIndex++] = b.verticesIndex + ind[i][2];
        //    }
        //    for (int i = 0; i < v.Length; i++)
        //        b.vertices[b.verticesIndex++] = new VertexPositionColorNormal(pos + v[i] * Radius, color, v[i]);
        //}

        //public void RenderSimple(PrimitiveBatcherOld batcher, Color color)
        //{
        //    var b = batcher.TriBatcher;

        //    var v = Icosahedron.Vertices;
        //    var ind = Icosahedron.Indices;

        //    b.EnsureAdditionalArrayCapacity(v.Length, ind.Length * 3);

        //    for (int i = 0; i < ind.Length; i++)
        //    {
        //        b.indices[b.indicesIndex++] = b.verticesIndex + ind[i][0];
        //        b.indices[b.indicesIndex++] = b.verticesIndex + ind[i][1];
        //        b.indices[b.indicesIndex++] = b.verticesIndex + ind[i][2];
        //    }
        //    for (int i = 0; i < v.Length; i++)
        //        b.vertices[b.verticesIndex++] = new VertexPositionColorNormal(pos + v[i] * Radius, color, v[i]);
        //}
    }
}
