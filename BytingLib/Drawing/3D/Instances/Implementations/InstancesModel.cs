namespace BytingLib
{
    public class InstancesModel : Instances<VertexInstanceTransformColor>
    {
        public void Draw(Matrix transform, Color color)
        {
            AddTransposed(new(Matrix.Transpose(transform), color));
        }
    }
}
