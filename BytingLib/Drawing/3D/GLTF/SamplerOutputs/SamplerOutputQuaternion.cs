namespace BytingLib
{
    public class SamplerOutputQuaternion : SamplerOutput<Quaternion>
    {
        public override Quaternion Interpolate(Quaternion value0, Quaternion value1, float interpolationAmount, SamplerFramesInterpolation interpolation)
        {
            Quaternion result;
            switch (interpolation)
            {
                case SamplerFramesInterpolation.Linear:
                    Quaternion.Lerp(ref value0, ref value1, interpolationAmount, out result);
                    return result;
                case SamplerFramesInterpolation.CubicSpline:
                    Quaternion.Slerp(ref value0, ref value1, interpolationAmount, out result);
                    return result;
                case SamplerFramesInterpolation.Step:
                default:
                    throw new NotImplementedException();
            }
        }

        protected override Quaternion[] BytesToValues(byte[] bytes)
        {
            return ByteExtension.ByteArrayToStructArray<Quaternion>(bytes, 3 * 4);
        }
    }
}
