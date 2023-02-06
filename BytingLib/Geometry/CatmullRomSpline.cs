using System;
using System.Collections.Generic;

namespace BytingLibGame.IngameSpline
{
    public static class CatmullRomSpline
    {
        public static int[] GetIndices(float t)
        {
            int tInt = (int)MathF.Floor(t);

            int[] indices = new int[4];
            for (int i = 0; i < indices.Length; i++)
            {
                indices[i] = tInt - 1 + i;
            }

            return indices;
        }

        public static int[] GetIndices(float t, int pointCount, bool looped)
        {
            int[] indices = GetIndices(t);

            for (int i = 0; i < 4; i++)
            {
                if (looped)
                {
                    indices[i] = (indices[i] + pointCount) % pointCount;
                }
                else
                {
                    if (indices[i] < 0)
                        indices[i] = 0;
                    if (indices[i] >= pointCount)
                        indices[i] = pointCount - 1;
                }
            }

            return indices;
        }

        public static float[] GetWeights(float t)
        {
            int tInt = (int)MathF.Floor(t);
            t = t - tInt;
            float tt = t * t;
            float ttt = tt * t;

            float[] weights = new float[4];
            weights[0] = 0.5f * (-ttt + 2f * tt - t);
            weights[1] = 0.5f * (3f * ttt - 5f * tt + 2f);
            weights[2] = 0.5f * (-3f * ttt + 4f * tt + t);
            weights[3] = 0.5f * (ttt - tt);

            return weights;
        }

        public static Vector2 Sample(IList<Vector2> points, float t, bool looped)
        {
            int[] indices = GetIndices(t, points.Count, looped);
            float[] weights = GetWeights(t);

            Vector2 result = Vector2.Zero;
            for (int i = 0; i < 4; i++)
                result += points[indices[i]] * weights[i];
            return result;
        }
        public static Vector3 Sample(IList<Vector3> points, float t, bool looped)
        {
            int[] indices = GetIndices(t, points.Count, looped);
            float[] weights = GetWeights(t);

            Vector3 result = Vector3.Zero;
            for (int i = 0; i < 4; i++)
                result += points[indices[i]] * weights[i];
            return result;
        }
        public static float Sample(IList<float> points, float t, bool looped)
        {
            int[] indices = GetIndices(t, points.Count, looped);
            float[] weights = GetWeights(t);

            float result = 0f;
            for (int i = 0; i < 4; i++)
                result += points[indices[i]] * weights[i];
            return result;
        }
        public static Quaternion Sample(IList<Quaternion> points, float t, bool looped)
        {
            int[] indices = GetIndices(t, points.Count, looped);
            float[] weights = GetWeights(t);

            Quaternion result = new Quaternion();
            for (int i = 0; i < 4; i++)
                result += points[indices[i]] * weights[i];
            return result;
        }
    }
}
