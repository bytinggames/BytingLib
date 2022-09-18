
using Microsoft.Xna.Framework;

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

        public object Clone() => MemberwiseClone();

        public BoundingBox GetBoundingBox()
        {
            throw new NotImplementedException();
        }
    }
}
