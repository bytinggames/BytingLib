namespace BytingLib
{
    public static class MathExtension
    {
        const double TwoPi = Math.PI * 2;

        public static float MinAbs(float val1, float val2)
        {
            return Math.Abs(val1) < Math.Abs(val2) ? val1 : val2;
        }
        public static float MaxAbs(float val1, float val2)
        {
            return Math.Abs(val1) > Math.Abs(val2) ? val1 : val2;
        }
        public static float AngleDistance(this float angleFrom, float angleTo)
        {
            angleFrom = angleFrom % MathHelper.TwoPi;
            if (angleFrom < 0)
            {
                angleFrom += MathHelper.TwoPi;
            }

            angleTo = angleTo % MathHelper.TwoPi;
            if (angleTo < 0)
            {
                angleTo += MathHelper.TwoPi;
            }

            float dist = angleTo - angleFrom;
            if (Math.Abs(dist) <= MathF.PI)
            {
                return dist;
            }
            else
            {
                return -Math.Sign(dist) * (MathHelper.TwoPi - Math.Abs(dist));
            }
        }
        public static float AngleDistanceAbs(this float angleFrom, float angleTo)
        {
            return MathF.Abs(AngleDistance(angleFrom, angleTo));
        }
        public static double AngleDistance(this double angleFrom, double angleTo)
        {
            angleFrom = angleFrom % TwoPi;
            if (angleFrom < 0)
            {
                angleFrom += TwoPi;
            }

            angleTo = angleTo % TwoPi;
            if (angleTo < 0)
            {
                angleTo += TwoPi;
            }

            double dist = angleTo - angleFrom;
            if (Math.Abs(dist) <= Math.PI)
            {
                return dist;
            }
            else
            {
                return -Math.Sign(dist) * (TwoPi - Math.Abs(dist));
            }
        }
        public static float GetMoveTo(this float val, float goal, float speed)
        {
            if (val == goal)
            {
                return val;
            }

            if (val < goal)
            {
                val += speed;
                if (val > goal)
                {
                    return goal;
                }

                return val;
            }
            else
            {
                val -= speed;
                if (val < goal)
                {
                    return goal;
                }

                return val;
            }
        }
        public static float GetMoveToAngle(this float val, float goal, float speed)
        {
            if (val == goal)
            {
                return val;
            }

            float dist = AngleDistance(val, goal);

            if (speed > Math.Abs(dist))
            {
                return goal;
            }
            else
            {
                return val + MathF.Sign(dist) * speed;
            }
        }
        public static float AverageAngle(IList<float> angles)
        {
            var x = angles.Sum(MathF.Cos) / angles.Count;
            var y = angles.Sum(MathF.Sin) / angles.Count;
            return MathF.Atan2(y, x);
        }

        public static float ToFovX(float fovY, float aspectRatio)
        {
            float a = MathF.Tan(fovY / 2f);
            a *= aspectRatio;
            return MathF.Atan(a) * 2f;
        }
        public static float ToFovY(float fovX, float aspectRatio)
        {
            float a = MathF.Tan(fovX / 2f);
            a /= aspectRatio;
            return MathF.Atan(a) * 2f;
        }

        public static float GetInterpolationFactor(float interpolationOnNormalElapsedRate, float elapsedFactor)
        {
            // example: interpolationFactor = 0.3
            // I went from interpolationFactor = 0.3 to 1-e^(-0.3566 * elapsedFactor) to make it fps independent
            // how did I get the number 0.3566?, I just set
            // 0.3 = 1-e^(-x)
            // -ln(0.7) = x
            float x = -MathF.Log(1f - interpolationOnNormalElapsedRate);
            x = 1f - MathF.Exp(-x * elapsedFactor);
            return x;
        }

        public static float GetDecayFactor(float factorOn60Ups, float deltaSeconds)
        {
            float decayPerSecond = MathF.Pow(factorOn60Ups, 60f);
            float decayThisFrame = MathF.Pow(decayPerSecond, deltaSeconds);
            return decayThisFrame;
        }
    }
}
