namespace BytingLib
{
    public class Gaussian
    {
        /// <summary>
        /// the array will have the size width * 2 + 1
        /// </summary>
        public static float[] GenerateKernel(int width, float sigma)
        {
            if (width == 0)
                return new float[] { 1 };

            float[] kernel = new float[width + 1 + width];

            float sum = 0f;

            for (int i = -width; i <= width; i++)
            {
                kernel[width + i] = MathF.Exp(-(i * i) / (2 * sigma * sigma)) / (MathF.PI * 2 * sigma * sigma);
                sum += kernel[width + i];
            }

            for (int i = 0; i < kernel.Length; i++)
            {
                kernel[i] /= sum;
            }

            return kernel;
        }
    }
}
