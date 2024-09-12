namespace BytingLib
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
                    {
                        indices[i] = 0;
                    }

                    if (indices[i] >= pointCount)
                    {
                        indices[i] = pointCount - 1;
                    }
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
            {
                result += points[indices[i]] * weights[i];
            }

            return result;
        }
        public static Vector3 Sample(IList<Vector3> points, float t, bool looped)
        {
            int[] indices = GetIndices(t, points.Count, looped);
            float[] weights = GetWeights(t);

            Vector3 result = Vector3.Zero;
            for (int i = 0; i < 4; i++)
            {
                result += points[indices[i]] * weights[i];
            }

            return result;
        }
        public static float Sample(IList<float> points, float t, bool looped)
        {
            int[] indices = GetIndices(t, points.Count, looped);
            float[] weights = GetWeights(t);

            float result = 0f;
            for (int i = 0; i < 4; i++)
            {
                result += points[indices[i]] * weights[i];
            }

            return result;
        }
        public static Quaternion Sample(IList<Quaternion> points, float t, bool looped)
        {
            int[] indices = GetIndices(t, points.Count, looped);
            float[] weights = GetWeights(t);

            Quaternion result = new Quaternion();
            for (int i = 0; i < 4; i++)
            {
                result += points[indices[i]] * weights[i];
            }

            return result;
        }


        #region Constant Distance Sampling

        // source: https://swharden.com/blog/2022-01-22-spline-interpolation/
        // edited by adding a looped bool parameter

        public static Vector3[] ResampleAtRoughlyConstantDistances(IList<Vector3> points, int count, bool looped)
        {
            if (looped)
            {
                points = points.ToList();
                points.Add(points[0]);
                points.Add(points[1]);
                points.Insert(0, points[points.Count - 3]);
            }

            int inputPointCount = points.Count;
            float[] inputDistances = new float[inputPointCount];
            for (int i = 1; i < inputPointCount; i++)
            {
                Vector3 d = points[i] - points[i - 1];
                float distance = d.Length();
                inputDistances[i] = inputDistances[i - 1] + distance;
            }

            float meanDistance;
            if (looped)
            {
                meanDistance = (inputDistances[inputDistances.Length - 2] - inputDistances[1]) / (count - 1);
            }
            else
            {
                meanDistance = inputDistances.Last() / (count - 1);
            }
            float[] evenDistances = Enumerable.Range(0, count).Select(x => x * meanDistance).ToArray();
            Vector3[] pointsOut = Interpolate(inputDistances, points, evenDistances, looped);
            return pointsOut;
        }

        private static Vector3[] Interpolate(float[] xOrig, IList<Vector3> yOrig, float[] xInterp, bool looped)
        {
            (Vector3[] a, Vector3[] b) = FitMatrix(xOrig, yOrig);

            if (looped)
            {
                float new0 = xOrig[1];
                for (int i = 0; i < xOrig.Length; i++)
                {
                    xOrig[i] -= new0;
                }
            }

            Vector3[] yInterp = new Vector3[xInterp.Length];
            int jStart = looped ? 1 : 0;
            int jEnd = looped ? xOrig.Length - 3 : xOrig.Length - 2;
            for (int i = 0; i < yInterp.Length; i++)
            {
                int j;
                for (j = jStart; j < jEnd; j++)
                {
                    if (xInterp[i] <= xOrig[j + 1])
                    {
                        break;
                    }
                }
                float dx = xOrig[j + 1] - xOrig[j];
                float t = (xInterp[i] - xOrig[j]) / dx;
                Vector3 y = (1 - t) * yOrig[j] + t * yOrig[j + 1] +
                    t * (1 - t) * (a[j] * (1 - t) + b[j] * t);
                yInterp[i] = y;
            }

            return yInterp;
        }

        private static (Vector3[] a, Vector3[] b) FitMatrix(float[] x, IList<Vector3> y)
        {
            int n = x.Length;
            Vector3[] a = new Vector3[n - 1];
            Vector3[] b = new Vector3[n - 1];
            Vector3[] r = new Vector3[n];
            float[] A = new float[n];
            float[] B = new float[n];
            float[] C = new float[n];

            float dx1, dx2;
            Vector3 dy1, dy2;

            dx1 = x[1] - x[0];
            C[0] = 1.0f / dx1;
            B[0] = 2.0f * C[0];
            r[0] = 3 * (y[1] - y[0]) / (dx1 * dx1);

            for (int i = 1; i < n - 1; i++)
            {
                dx1 = x[i] - x[i - 1];
                dx2 = x[i + 1] - x[i];
                A[i] = 1.0f / dx1;
                C[i] = 1.0f / dx2;
                B[i] = 2.0f * (A[i] + C[i]);
                dy1 = y[i] - y[i - 1];
                dy2 = y[i + 1] - y[i];
                r[i] = 3 * (dy1 / (dx1 * dx1) + dy2 / (dx2 * dx2));
            }

            dx1 = x[n - 1] - x[n - 2];
            dy1 = y[n - 1] - y[n - 2];
            A[n - 1] = 1.0f / dx1;
            B[n - 1] = 2.0f * A[n - 1];
            r[n - 1] = 3 * (dy1 / (dx1 * dx1));

            float[] cPrime = new float[n];
            cPrime[0] = C[0] / B[0];
            for (int i = 1; i < n; i++)
            {
                cPrime[i] = C[i] / (B[i] - cPrime[i - 1] * A[i]);
            }

            Vector3[] dPrime = new Vector3[n];
            dPrime[0] = r[0] / B[0];
            for (int i = 1; i < n; i++)
            {
                dPrime[i] = (r[i] - dPrime[i - 1] * A[i]) / (B[i] - cPrime[i - 1] * A[i]);
            }

            Vector3[] k = new Vector3[n];
            k[n - 1] = dPrime[n - 1];
            for (int i = n - 2; i >= 0; i--)
            {
                k[i] = dPrime[i] - cPrime[i] * k[i + 1];
            }

            for (int i = 1; i < n; i++)
            {
                dx1 = x[i] - x[i - 1];
                dy1 = y[i] - y[i - 1];
                a[i - 1] = k[i - 1] * dx1 - dy1;
                b[i - 1] = -k[i] * dx1 + dy1;
            }

            return (a, b);
        }

        #endregion
    }
}
