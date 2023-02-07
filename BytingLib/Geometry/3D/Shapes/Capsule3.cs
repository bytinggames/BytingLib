namespace BytingLib
{
    public class Capsule3 : IShape3
    {
        private Vector3 sphereDistance;
        public Sphere3[] Spheres { get; private set; }
        public AxisRadius3 AxisRadius { get; private set; }

        public Capsule3(Vector3 pos, Vector3 sphereDistance, float radius)
        {
            Spheres = new Sphere3[]
            {
                new Sphere3(pos, radius),
                new Sphere3(pos + sphereDistance, radius)
            };

            AxisRadius = new AxisRadius3(pos, Vector3.Zero /* will be set with SphereDistance */, radius);

            SphereDistance = sphereDistance;
        }

        public Type GetCollisionType() => typeof(Capsule3);

        public Vector3 Pos
        {
            get => Spheres[0].Pos;
            set
            {
                AxisRadius.Pos = Spheres[0].Pos = value;
                Spheres[1].Pos = value + SphereDistance;
            }
        }
        public float X { get => Spheres[0].X; set => Spheres[0].X = value; }
        public float Y { get => Spheres[0].Y; set => Spheres[0].Y = value; }
        public float Z { get => Spheres[0].Z; set => Spheres[0].Z = value; }

        public Sphere3 Sphere0 => Spheres[0];
        public Sphere3 Sphere1 => Spheres[1];

        /// <summary>Gets or sets the distance between the origin of both spheres.</summary>
        public Vector3 SphereDistance
        {
            get => sphereDistance;
            set
            {
                sphereDistance = value;
                Spheres[1].Pos = Pos + value;
                AxisRadius.Dir = Vector3.Normalize(sphereDistance);
            }
        }
        /// <summary>Gets or sets the x-distance between the origin of both spheres.</summary>
        public float SphereDistanceX
        {
            get => sphereDistance.X;
            set
            {
                sphereDistance.X = value;
                Spheres[1].X = Pos.X + value;
                AxisRadius.Dir = Vector3.Normalize(sphereDistance);
            }
        }
        /// <summary>Gets or sets the y-distance between the origin of both spheres.</summary>
        public float SphereDistanceY
        {
            get => sphereDistance.Y;
            set
            {
                sphereDistance.Y = value;
                Spheres[1].Y = Pos.Y + value;
                AxisRadius.Dir = Vector3.Normalize(sphereDistance);
            }
        }
        /// <summary>Gets or sets the z-distance between the origin of both spheres.</summary>
        public float SphereDistanceZ
        {
            get => sphereDistance.Z;
            set
            {
                sphereDistance.Z = value;
                Spheres[1].Z = Pos.Z + value;
                AxisRadius.Dir = Vector3.Normalize(sphereDistance);
            }
        }
        public Vector3 DistanceN => AxisRadius.Dir;

        public float Radius
        {
            get => Spheres[0].Radius;
            set
            {
                AxisRadius.Radius = Spheres[0].Radius = Spheres[1].Radius = value;
            }
        }

        /// <summary>Gets the absolute position of the second sphere.</summary>
        public Vector3 Pos2 => Pos + SphereDistance;

        public virtual object Clone()
        {
            Capsule3 clone = (Capsule3)MemberwiseClone();

            clone.AxisRadius = (AxisRadius3)AxisRadius.Clone();
            clone.Spheres = new Sphere3[]
            {
                (Sphere3)Sphere0.Clone(),
                (Sphere3)Sphere1.Clone()
            };

            return clone;
        }

        public BoundingBox GetBoundingBox()
        {
            BoundingBox box = BoundingBox.CreateFromPoints(new Vector3[] { Pos, Pos2 });
            box.Min -= new Vector3(Radius);
            box.Max += new Vector3(Radius);
            return box;
        }
    }
}
