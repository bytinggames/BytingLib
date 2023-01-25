namespace BytingLib
{
    public static class DrawHelper
    {
        public static int RadiusToVertexCount(float radius)
        {
            return Math.Max(4, (int)(3.7f * MathF.Pow(1.45f, MathF.Log(MathF.Abs(radius), 2)))); // see desmos
        }
    }
}
