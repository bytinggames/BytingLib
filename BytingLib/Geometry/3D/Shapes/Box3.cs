
using BytingLib.DataTypes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

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
                t.Translation + (t.Right + t.Up + t.Backward) / 2f,
                t.Translation + (t.Right + t.Up - t.Backward) / 2f,
                t.Translation + (t.Right - t.Up + t.Backward) / 2f,
                t.Translation + (t.Right - t.Up - t.Backward) / 2f,
                t.Translation - (t.Right + t.Up + t.Backward) / 2f,
                t.Translation - (t.Right + t.Up - t.Backward) / 2f,
                t.Translation - (t.Right - t.Up + t.Backward) / 2f,
                t.Translation - (t.Right - t.Up - t.Backward) / 2f,
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

        public IEnumerable<Triangle3> Triangulate()
        {
            var corners = GetCorners();
            yield return new Triangle3(corners[7], corners[5], corners[3]);
            yield return new Triangle3(corners[7], corners[3], corners[6]);
            yield return new Triangle3(corners[7], corners[6], corners[5]);

            yield return new Triangle3(corners[4], corners[5], corners[6]);
            yield return new Triangle3(corners[4], corners[6], corners[0]);
            yield return new Triangle3(corners[4], corners[0], corners[5]);

            yield return new Triangle3(corners[2], corners[6], corners[3]);
            yield return new Triangle3(corners[2], corners[3], corners[0]);
            yield return new Triangle3(corners[2], corners[0], corners[6]);

            yield return new Triangle3(corners[1], corners[3], corners[5]);
            yield return new Triangle3(corners[1], corners[5], corners[0]);
            yield return new Triangle3(corners[1], corners[0], corners[3]);
        }

        //public void Render(PrimitiveBatcherOld batcher, Color color)
        //{
        //    var b = batcher.TriBatcher;

        //    const int faces = 6;
        //    // draw 6 quads, where each quad has 6 indices and 4 vertices
        //    b.EnsureAdditionalArrayCapacity(4 * faces, 6 * faces);

        //    // indices
        //    // 3 - 2
        //    // | / |
        //    // 0 - 1
        //    int startIndex = b.verticesIndex;
        //    for (int face = 0; face < faces; face++)
        //    {
        //        b.indices[b.indicesIndex++] = startIndex + 0;
        //        b.indices[b.indicesIndex++] = startIndex + 2;
        //        b.indices[b.indicesIndex++] = startIndex + 1;
        //        b.indices[b.indicesIndex++] = startIndex + 0;
        //        b.indices[b.indicesIndex++] = startIndex + 3;
        //        b.indices[b.indicesIndex++] = startIndex + 2;
        //        startIndex += 4;
        //    }

        //    var t = _transform;
        //    Vector3 X = t.Right / 2f;
        //    Vector3 Y = t.Up / 2f;
        //    Vector3 Z = t.Backward / 2f;

        //    Vector3 NX = Vector3.Normalize(t.Right);
        //    Vector3 NY = Vector3.Normalize(t.Up);
        //    Vector3 NZ = Vector3.Normalize(t.Backward);

        //    var p = t.Translation;

        //    // +z face
        //    b.vertices[b.verticesIndex++] = new(p - X - Y + Z, color, NZ);
        //    b.vertices[b.verticesIndex++] = new(p + X - Y + Z, color, NZ);
        //    b.vertices[b.verticesIndex++] = new(p + X + Y + Z, color, NZ);
        //    b.vertices[b.verticesIndex++] = new(p - X + Y + Z, color, NZ);

        //    // +y face
        //    b.vertices[b.verticesIndex++] = new(p - X + Y + Z, color, NY);
        //    b.vertices[b.verticesIndex++] = new(p + X + Y + Z, color, NY);
        //    b.vertices[b.verticesIndex++] = new(p + X + Y - Z, color, NY);
        //    b.vertices[b.verticesIndex++] = new(p - X + Y - Z, color, NY);

        //    // -z face
        //    b.vertices[b.verticesIndex++] = new(p - X + Y - Z, color, -NZ);
        //    b.vertices[b.verticesIndex++] = new(p + X + Y - Z, color, -NZ);
        //    b.vertices[b.verticesIndex++] = new(p + X - Y - Z, color, -NZ);
        //    b.vertices[b.verticesIndex++] = new(p - X - Y - Z, color, -NZ);

        //    // -y face
        //    b.vertices[b.verticesIndex++] = new(p - X - Y - Z, color, -NY);
        //    b.vertices[b.verticesIndex++] = new(p + X - Y - Z, color, -NY);
        //    b.vertices[b.verticesIndex++] = new(p + X - Y + Z, color, -NY);
        //    b.vertices[b.verticesIndex++] = new(p - X - Y + Z, color, -NY);

        //    // +x face
        //    b.vertices[b.verticesIndex++] = new(p + X - Y + Z, color, NX);
        //    b.vertices[b.verticesIndex++] = new(p + X - Y - Z, color, NX);
        //    b.vertices[b.verticesIndex++] = new(p + X + Y - Z, color, NX);
        //    b.vertices[b.verticesIndex++] = new(p + X + Y + Z, color, NX);

        //    // -x face
        //    b.vertices[b.verticesIndex++] = new(p - X - Y - Z, color, -NX);
        //    b.vertices[b.verticesIndex++] = new(p - X - Y + Z, color, -NX);
        //    b.vertices[b.verticesIndex++] = new(p - X + Y + Z, color, -NX);
        //    b.vertices[b.verticesIndex++] = new(p - X + Y - Z, color, -NX);

        //}
    }
}
