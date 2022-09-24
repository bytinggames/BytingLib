
using Microsoft.Xna.Framework;

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
                t.Translation + t.Right + t.Up + t.Backward,
                t.Translation + t.Right + t.Up - t.Backward,
                t.Translation + t.Right - t.Up + t.Backward,
                t.Translation + t.Right - t.Up - t.Backward,
                t.Translation - t.Right + t.Up + t.Backward,
                t.Translation - t.Right + t.Up - t.Backward,
                t.Translation - t.Right - t.Up + t.Backward,
                t.Translation - t.Right - t.Up - t.Backward,
            };
        }

        public AABB3 GetAABBWithoutRotation()
        {
            _transform.Decompose(out Vector3 scale, out _, out _);
            scale = scale.GetAbs();
            return new AABB3(Pos, scale * 2f);
        }
        public AABB3 GetAABBWithoutRotationAndTranslation()
        {
            _transform.Decompose(out Vector3 scale, out _, out _);
            scale = scale.GetAbs();
            return new AABB3(Vector3.Zero, scale * 2f);
        }
    }
}
