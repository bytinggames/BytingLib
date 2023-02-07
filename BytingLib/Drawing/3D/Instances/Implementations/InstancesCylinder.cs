namespace BytingLib
{
    public class InstancesCylinder : Instances<VertexInstanceTransformColor>
    {
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
    }
}
