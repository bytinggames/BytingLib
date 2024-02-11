namespace BytingLib
{
    public static class QuaternionExtension
    {
        /// <summary>
        /// Seems not to work with some quaternions... Maybe when coming from a scaled matrix.
        /// Use <see cref="MatrixExtension.RotatesBaseAxisToParallelBaseAxis"/> instead.
        /// </summary>
        [Obsolete]
        public static bool RotatesBaseAxisToParallelBaseAxis(this Quaternion q, float precision = 0.0001f)
        {
            Matrix m = Matrix.CreateFromQuaternion(q);
            return Check(Vector3.Dot(m.Right, Vector3.Right), precision)
                && Check(Vector3.Dot(m.Right, Vector3.Up), precision)
                && Check(Vector3.Dot(m.Up, Vector3.Right), precision)
                && Check(Vector3.Dot(m.Up, Vector3.Up), precision);

            bool Check(float dot, float precision)
            {
                dot = MathF.Abs(dot);
                return dot <= precision || dot >= 1f - precision;
            }
        }

    }
}
