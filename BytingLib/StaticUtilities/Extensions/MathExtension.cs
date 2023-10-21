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
    }
}
