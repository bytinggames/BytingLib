using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BytingLib
{
    public class InstancesModel : Instances<VertexInstanceTransformColor>
    {
        public void Draw(Matrix transform, Color color)
        {
            Add(new(Matrix.Transpose(transform), color));
        }
    }
}
