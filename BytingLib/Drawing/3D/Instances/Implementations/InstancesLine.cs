namespace BytingLib
{
    public class InstancesLine : Instances<VertexInstanceTransformColor>
    {
        float infinity;

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

        public void Draw(Point3 point, Color color, float size)
        {
            Vector3 dir = new Vector3(size, size, size);
            AddTransposed(new(ToRenderTransform(point.Pos - dir, dir * 2f), color));
            dir = new Vector3(-size, size, size);
            AddTransposed(new(ToRenderTransform(point.Pos - dir, dir * 2f), color));
            dir = new Vector3(size, -size, size);
            AddTransposed(new(ToRenderTransform(point.Pos - dir, dir * 2f), color));
            dir = new Vector3(size, size, -size);
            AddTransposed(new(ToRenderTransform(point.Pos - dir, dir * 2f), color));
        }

        private static Matrix ToRenderTransform(Line3 line)
            => ToRenderTransform(line.Pos, line.Dir);
        private static Matrix ToRenderTransform(Vector3 pos, Vector3 dir)
        {
            return Matrix.Transpose(new Matrix(new Vector4(dir, 0),
                            Vector4.Zero,
                            Vector4.Zero,
                            new Vector4(pos, 1)));
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
