namespace BytingLib
{
    public class InstancesOpenCylinder : Instances<VertexInstanceTransformColor>
    {
        float infinity;

        public InstancesOpenCylinder(float infinity)
        {
            this.infinity = infinity;
        }

        public void Draw(AxisRadius3 axisRadius, Color color)
        {
            AddTransposed(new(ToRenderTransform(axisRadius), color));
        }
        private Matrix ToRenderTransform(AxisRadius3 axisRadius)
        {
            Vector3 axis1 = Vector3.Normalize(Vector3.Cross(axisRadius.Dir.GetNonParallelVector(), axisRadius.Dir));
            Vector3 axis2 = Vector3.Normalize(Vector3.Cross(axis1, axisRadius.Dir));

            return Matrix.Transpose(new Matrix(new Vector4(axisRadius.Dir * infinity * 2f, 0),
                new Vector4(axis1 * axisRadius.Radius, 0),
                new Vector4(axis2 * axisRadius.Radius, 0),
                new Vector4(axisRadius.Pos - axisRadius.Dir * infinity, 1)));
        }

        public void Draw(Cylinder3 cylinder, Color color)
        {
            AddTransposed(new(ToRenderTransform(cylinder), color));
        }
        private Matrix ToRenderTransform(Cylinder3 cylinder)
        {
            Vector3 axis1 = Vector3.Normalize(Vector3.Cross(cylinder.Length.GetNonParallelVector(), cylinder.Length));
            Vector3 axis2 = Vector3.Normalize(Vector3.Cross(axis1, cylinder.Length));

            return Matrix.Transpose(new Matrix(new Vector4(cylinder.Length, 0),
                new Vector4(axis1 * cylinder.Radius, 0),
                new Vector4(axis2 * cylinder.Radius, 0),
                new Vector4(cylinder.Pos, 1)));
        }

        public void Draw(AxisRadius3 axisRadius, Color color, Vector3 length)
        {
            AddTransposed(new(ToRenderTransform(axisRadius, length), color));
        }
        private Matrix ToRenderTransform(AxisRadius3 axisRadius, Vector3 length)
        {
            Vector3 axis1 = Vector3.Normalize(Vector3.Cross(length.GetNonParallelVector(), length));
            Vector3 axis2 = Vector3.Normalize(Vector3.Cross(axis1, length));

            return Matrix.Transpose(new Matrix(new Vector4(length, 0),
                new Vector4(axis1 * axisRadius.Radius, 0),
                new Vector4(axis2 * axisRadius.Radius, 0),
                new Vector4(axisRadius.Pos, 1)));
        }
    }
}
