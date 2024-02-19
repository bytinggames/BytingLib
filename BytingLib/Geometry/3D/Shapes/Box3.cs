namespace BytingLib
{
    public class Box3 : IShape3
    {
        private Matrix _transform;
        private Matrix _transformInverse;

        public Box3(Matrix transform)
        {
            Transform = transform;
        }

        public Matrix Transform
        {
            get => _transform;
            set
            {
                _transform = value;
                TransformChanged();
            }
        }
        private void TransformChanged()
        {
            _transformInverse = Matrix.Invert(_transform);
        }

        public Matrix TransformInverse => _transformInverse;

        /// <summary>Center of the Box.</summary>
        public Vector3 Pos
        {
            get => Transform.Translation;
            set
            {
                _transform.Translation = value;
                TransformChanged();
            }
        }
        public float X
        {
            get => Transform.Translation.X;
            set
            {
                _transform.Translation = new Vector3(value, _transform.Translation.Y, _transform.Translation.Z);
                TransformChanged();
            }
        }
        public float Y
        {
            get => Transform.Translation.Y;
            set
            {
                _transform.Translation = new Vector3(_transform.Translation.X, value, _transform.Translation.Z);
                TransformChanged();
            }
        }
        public float Z
        {
            get => Transform.Translation.Z;
            set
            {
                _transform.Translation = new Vector3(_transform.Translation.X, _transform.Translation.Y, value);
                TransformChanged();
            }
        }

        public Type GetCollisionType() => typeof(Box3);

        public virtual object Clone() => MemberwiseClone();

        public BoundingBox GetBoundingBox()
        {
            return BoundingBox.CreateFromPoints(GetCorners());
        }

        public List<Vector3> GetCorners()
        {
            var t = _transform;
            return new List<Vector3>()
            {
                t.Translation - (t.Right - t.Up - t.Backward) / 2f,
                t.Translation + (t.Right + t.Up + t.Backward) / 2f,
                t.Translation + (t.Right - t.Up + t.Backward) / 2f,
                t.Translation - (t.Right + t.Up - t.Backward) / 2f,
                t.Translation - (t.Right - t.Up + t.Backward) / 2f,
                t.Translation + (t.Right + t.Up - t.Backward) / 2f,
                t.Translation + (t.Right - t.Up - t.Backward) / 2f,
                t.Translation - (t.Right + t.Up + t.Backward) / 2f,
            };
        }

        public AABB3 GetAABBWithoutRotation()
        {
            Vector3 scale = _transform.GetScale();
            scale = scale.GetAbs();
            return new AABB3(Pos, scale * 2f);
        }
        public AABB3 GetAABBWithoutRotationAndTranslation()
        {
            Vector3 scale = _transform.GetScale();
            scale = scale.GetAbs();
            return new AABB3(-scale, scale);
        }

        public IEnumerable<Triangle3> Triangulate()
        {
            var c = GetCorners();

            yield return new Triangle3(c[4], c[3], c[0]);
            yield return new Triangle3(c[3], c[4], c[7]);

            yield return new Triangle3(c[2], c[5], c[1]);
            yield return new Triangle3(c[5], c[2], c[6]);

            yield return new Triangle3(c[0], c[2], c[1]);
            yield return new Triangle3(c[2], c[0], c[3]);

            yield return new Triangle3(c[6], c[4], c[5]);
            yield return new Triangle3(c[4], c[6], c[7]);

            yield return new Triangle3(c[2], c[7], c[6]);
            yield return new Triangle3(c[7], c[2], c[3]);

            yield return new Triangle3(c[4], c[1], c[5]);
            yield return new Triangle3(c[1], c[4], c[0]);
        }
    }
}
