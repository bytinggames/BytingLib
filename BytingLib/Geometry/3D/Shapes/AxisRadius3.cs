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
            {
                boundingSize.X = float.PositiveInfinity;
            }
            else
            {
                boundingSize.X = Radius;
            }

            if (Dir.Y != 0)
            {
                boundingSize.Y = float.PositiveInfinity;
            }
            else
            {
                boundingSize.Y = Radius;
            }

            if (Dir.Z != 0)
            {
                boundingSize.Z = float.PositiveInfinity;
            }
            else
            {
                boundingSize.Z = Radius;
            }

            return new BoundingBox(Pos - boundingSize, Pos + boundingSize);
        }
    }
}
