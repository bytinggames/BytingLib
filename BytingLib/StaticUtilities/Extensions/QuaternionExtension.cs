namespace BytingLib
{
    public static class QuaternionExtension
    {
        public static bool RotatesBaseAxisToParallelBaseAxis(this Quaternion q, float precision = 0.0001f)
        {
            Matrix m = Matrix.CreateFromQuaternion(q);
            return Check(Vector3.Dot(m.Right, Vector3.Right), precision)
                && Check(Vector3.Dot(m.Right, Vector3.Up), precision)
                && Check(Vector3.Dot(m.Up, Vector3.Right), precision)
                && Check(Vector3.Dot(m.Up, Vector3.Up), precision);
        }

        private static bool Check(float dot, float precision)
        {
            dot = MathF.Abs(dot);
            return dot <= precision || dot >= 1f - precision;
        }
    }
}
