
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BytingLib
{
    public class Ray3 : IShape3
    {
        private Vector3 pos;
        /// <summary>
        /// this vector should never be set to a non-normalized vector
        /// </summary>
        public Vector3 Dir;

        public Ray3(Vector3 position, Vector3 normalizedDirection)
        {
            pos = position;
            Dir = normalizedDirection;
        }

        public Vector3 Pos { get => pos; set => pos = value; }
        public float X { get => pos.X; set => pos.X = value; }
        public float Y { get => pos.Y; set => pos.Y = value; }
        public float Z { get => pos.Z; set => pos.Z = value; }

        public Type GetCollisionType() => typeof(Ray3);

        public virtual object Clone() => MemberwiseClone();

        public BoundingBox GetBoundingBox()
        {
            throw new NotImplementedException();
        }

        //public void Render(PrimitiveBatcherOld batcher, Color color)
        //{
        //    Render(batcher, color, 1000f);
        //}

        //public void Render(PrimitiveBatcherOld batcher, Color color, float lineLength)
        //{
        //    var b = batcher.LineBatcher;

        //    // draw two lines originating from the origin
        //    b.EnsureAdditionalArrayCapacity(2, 2);

        //    // indices
        //    // 0 - 1
        //    b.indices[b.indicesIndex++] = b.verticesIndex + 0;
        //    b.indices[b.indicesIndex++] = b.verticesIndex + 1;

        //    // line: pos - end
        //    b.vertices[b.verticesIndex++] = new(pos, color);
        //    b.vertices[b.verticesIndex++] = new(pos + Dir * lineLength, color);
        //}
    }
}
