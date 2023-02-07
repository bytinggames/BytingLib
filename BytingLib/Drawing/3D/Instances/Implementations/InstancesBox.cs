namespace BytingLib
{
    public class InstancesBox : Instances<VertexInstanceTransformColor>
    {
        public void Draw(Box3 box, Color color)
        {
            AddTransposed(new(Matrix.Transpose(box.Transform), color));
        }

        public void Draw(AABB3 aabb, Color color)
        {
            Draw(aabb.ToBox(), color);
        }
    }
}
