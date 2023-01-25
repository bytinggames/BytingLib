namespace BytingLib
{
    public class InstancesTriangle : Instances<VertexInstanceTransformColor>
    {
        public void Draw(Triangle3 triangle, Color color)
        {
            Add(new(ToRenderTransform(triangle), color));
        }

        private static Matrix ToRenderTransform(Triangle3 tri)
        {
            return Matrix.Transpose(new Matrix(new Vector4(tri.DirB, 0),
                            new Vector4(tri.DirA, 0),
                            new Vector4(tri.N, 0),
                            new Vector4(tri.Pos, 1)));
        }
    }
}
