namespace BytingLib
{
    public class InstancesLine : Instances<VertexInstanceTransformColor>
    {
        float infinity = 1000f;

        public InstancesLine(float infinity)
        {
            this.infinity = infinity;
        }

        public void Draw(Line3 line, Color color)
        {
            AddTransposed(new(ToRenderTransform(line), color));
        }

        public void Draw(Axis3 axis, Color color)
        {
            AddTransposed(new(ToRenderTransform(axis), color));
        }
        public void Draw(Ray3 ray, Color color)
        {
            AddTransposed(new(ToRenderTransform(ray), color));
        }

        private static Matrix ToRenderTransform(Line3 line)
        {
            return Matrix.Transpose(new Matrix(new Vector4(line.Dir, 0),
                            Vector4.Zero,
                            Vector4.Zero,
                            new Vector4(line.Pos, 1)));
        }
        private Matrix ToRenderTransform(Axis3 axis)
        {
            return Matrix.Transpose(new Matrix(new Vector4(axis.Dir * infinity * 2f, 0),
                            Vector4.Zero,
                            Vector4.Zero,
                            new Vector4(axis.Pos - axis.Dir * infinity, 1)));
        }
        private Matrix ToRenderTransform(Ray3 ray)
        {
            return Matrix.Transpose(new Matrix(new Vector4(ray.Dir * infinity, 0),
                            Vector4.Zero,
                            Vector4.Zero,
                            new Vector4(ray.Pos, 1)));
        }
    }
}
