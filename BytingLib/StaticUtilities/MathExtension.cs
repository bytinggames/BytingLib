
using Microsoft.Xna.Framework;

namespace BytingLib
{
    public static class MathExtension
    {
        public static float MinAbs(float val1, float val2)
        {
            return Math.Abs(val1) < Math.Abs(val2) ? val1 : val2;
        }
        public static float MaxAbs(float val1, float val2)
        {
            return Math.Abs(val1) > Math.Abs(val2) ? val1 : val2;
        }
        public static float AngleDistance(float angleFrom, float angleTo)
        {
            angleFrom = angleFrom % MathHelper.TwoPi;
            if (angleFrom < 0)
                angleFrom += MathHelper.TwoPi;

            angleTo = angleTo % MathHelper.TwoPi;
            if (angleTo < 0)
                angleTo += MathHelper.TwoPi;

            float dist = angleTo - angleFrom;
            if (Math.Abs(dist) <= Math.PI)
                return dist;
            else
                return -Math.Sign(dist) * (MathHelper.TwoPi - Math.Abs(dist));
        }
    }
}
