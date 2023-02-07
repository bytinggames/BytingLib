namespace BytingLib
{
    public class InstancesSphere : Instances<VertexInstanceTransformColor>
    {
        public void Draw(Sphere3 sphere, Color color)
        {
            AddTransposed(new(ToRenderTransform(sphere), color));
        }

        private static Matrix ToRenderTransform(Sphere3 sphere)
        {
            return Matrix.Transpose(
                Matrix.CreateScale(sphere.Radius)
                * Matrix.CreateTranslation(sphere.Pos)
                );
        }
    }
}
