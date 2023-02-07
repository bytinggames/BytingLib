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
    }
}
