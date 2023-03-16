namespace BytingLib
{
    public class InstancesTriangle : Instances<VertexInstanceTransformColor>
    {
        private readonly float infinity;

        public InstancesTriangle(float infinity)
        {
            this.infinity = infinity;
        }

        public void Draw(Vector3 v1, Vector3 v2, Vector3 v3, Color color)
        {
            AddTransposed(new(ToRenderTransform(v1, v2, v3), color));
        }

        public void Draw(Triangle3 triangle, Color color)
        {
            AddTransposed(new(ToRenderTransform(triangle), color));
        }

        public void Draw(Plane3 plane, Color color)
            => Draw(plane, color, infinity);

        public void Draw(Plane3 plane, Color color, float infinity)
        {
            Vector3 axis1 = Vector3.Normalize(Vector3.Cross(plane.Normal, plane.Normal.GetNonParallelVector()));
            Vector3 axis2 = Vector3.Normalize(Vector3.Cross(plane.Normal, axis1)); // PERFORMANCE: normalize might not be needed here?

            Vector3 v1 = plane.Pos - axis1 * infinity;
            Vector3 v2 = plane.Pos + axis1 * infinity;
            Draw(v1, plane.Pos + axis2 * infinity, v2, color);
            Draw(v1, v2, plane.Pos - axis2 * infinity, color);
        }
        public void Draw(Plane3 plane, Color color, float infinity, Vector3 axis1)
        {
            Vector3 axis2 = Vector3.Normalize(Vector3.Cross(plane.Normal, axis1)); // PERFORMANCE: normalize might not be needed here?
            axis1 = Vector3.Normalize(Vector3.Cross(axis2, plane.Normal)); // make sure it really is perpendicular and normalized

            Vector3 v1 = plane.Pos - axis1 * infinity;
            Vector3 v2 = plane.Pos + axis1 * infinity;
            Draw(v1, plane.Pos + axis2 * infinity, v2, color);
            Draw(v1, v2, plane.Pos - axis2 * infinity, color);
        }

        private static Matrix ToRenderTransform(Triangle3 tri)
        {
            return Matrix.Transpose(new Matrix(new Vector4(tri.DirB, 0),
                            new Vector4(tri.DirA, 0),
                            new Vector4(tri.N, 0),
                            new Vector4(tri.Pos, 1)));
        }
        private static Matrix ToRenderTransform(Vector3 v1, Vector3 v2, Vector3 v3)
        {
            Vector3 dirA = v2 - v1;
            Vector3 dirB = v3 - v1;
            return Matrix.Transpose(new Matrix(new Vector4(dirB, 0),
                            new Vector4(dirA, 0),
                            new Vector4(Vector3.Normalize(Vector3.Cross(dirB, dirA)), 0),
                            new Vector4(v1, 1)));
        }
    }
}
