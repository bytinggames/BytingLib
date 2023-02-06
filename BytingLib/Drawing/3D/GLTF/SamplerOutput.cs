using BytingLibGame.IngameSpline;

namespace BytingLib
{
    public abstract class SamplerOutput<T>
    {
        protected T[]? values;

        public virtual T GetValue(int frame) => values![frame];
        public virtual T GetValueLinear(int frame0, int frame1, float interpolationAmount)
        {
            return InterpolateLinear(values![frame0], values[frame1], interpolationAmount);
        }
        public virtual T GetValueCubicSpline(int[] frames, float interpolationAmount)
        {
            float[] weights = CatmullRomSpline.GetWeights(interpolationAmount);
            return InterpolateCubicSpline(frames!.Select(f => values![f]).ToArray(), weights);
        }

        public abstract T InterpolateLinear(T value0, T value1, float interpolationAmount);
        public abstract T InterpolateCubicSpline(T[] values4, float[] weights);
        internal void Initialize(byte[] bytes, int keyFrameCount)
        {
            values = BytesToValues(bytes, keyFrameCount);
        }

        protected abstract T[] BytesToValues(byte[] bytes, int keyFrameCount);
    }
}
