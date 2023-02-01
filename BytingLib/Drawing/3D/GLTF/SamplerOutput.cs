namespace BytingLib
{
    public abstract class SamplerOutput<T>
    {
        protected T[]? values;

        public virtual T GetValue(int frame) => values![frame];
        public virtual T GetValue(int frame0, int frame1, float interpolationAmount, SamplerFramesInterpolation interpolation)
        {
            return Interpolate(values![frame0], values[frame1], interpolationAmount, interpolation);
        }

        public abstract T Interpolate(T value0, T value1, float interpolationAmount, SamplerFramesInterpolation interpolation);
        internal void Initialize(byte[] bytes, int keyFrameCount)
        {
            values = BytesToValues(bytes, keyFrameCount);
        }

        protected abstract T[] BytesToValues(byte[] bytes, int keyFrameCount);
    }
}
