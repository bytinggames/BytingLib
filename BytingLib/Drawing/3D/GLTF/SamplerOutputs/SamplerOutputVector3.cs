namespace BytingLib
{
    public class SamplerOutputVector3 : SamplerOutput<Vector3>
    {
        public override Vector3 Interpolate(Vector3 value0, Vector3 value1, float interpolationAmount, SamplerFramesInterpolation interpolation)
        {
            switch (interpolation)
            {
                case SamplerFramesInterpolation.Linear:
                    Vector3.Lerp(ref value0, ref value1, interpolationAmount, out Vector3 result);
                    return result;
                case SamplerFramesInterpolation.Step:
                case SamplerFramesInterpolation.CubicSpline:
                default:
                    throw new NotImplementedException();
            }
        }

        protected override Vector3[] BytesToValues(byte[] bytes)
        {
            return ByteExtension.ByteArrayToStructArray<Vector3>(bytes, 3 * 4);
        }
    }
}
