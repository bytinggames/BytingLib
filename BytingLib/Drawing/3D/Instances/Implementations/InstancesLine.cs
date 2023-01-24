using Microsoft.Xna.Framework;

namespace BytingLib
{
    public class InstancesLine : Instances<VertexInstanceTransformColor>
    {
        public void Draw(Line3 line, Color color)
        {
            Add(new(ToRenderTransform(line), color));
        }

        private static Matrix ToRenderTransform(Line3 line)
        {
            return Matrix.Transpose(new Matrix(new Vector4(line.Dir, 0),
                            Vector4.Zero,
                            Vector4.Zero,
                            new Vector4(line.Pos, 1)));
        }
    }
}
