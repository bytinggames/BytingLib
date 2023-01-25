namespace BytingLib
{
    public static class RandomExtension
    {
        public static Color NextColor(this Random rand)
        {
            return new Color(rand.Next(256), rand.Next(256), rand.Next(256));
        }
        public static Color NextColorTransparent(this Random rand)
        {
            return new Color(rand.Next(256), rand.Next(256), rand.Next(256), rand.Next(256));
        }
        public static float NextSingle(this Random rand, float minValue, float maxValue)
        {
            float f = rand.NextSingle();
            f *= maxValue - minValue;
            f += minValue;
            return f;
        }
        public static Vector2 NextVector2(this Random rand)
        {
            return new Vector2(rand.NextSingle(),
                               rand.NextSingle());
        }
        public static Vector3 NextVector3(this Random rand)
        {
            return new Vector3(rand.NextSingle(),
                               rand.NextSingle(),
                               rand.NextSingle());
        }
        public static Vector2 NextVector2Box(this Random rand)
        {
            return new Vector2(rand.NextSingle() * 2f - 1f,
                               rand.NextSingle() * 2f - 1f);
        }
        public static Vector3 NextVector3Box(this Random rand)
        {
            return new Vector3(rand.NextSingle() * 2f - 1f,
                               rand.NextSingle() * 2f - 1f,
                               rand.NextSingle() * 2f - 1f);
        }
        public static Vector2 NextVector2(this Random rand, float minValue, float maxValue)
        {
            return new Vector2(rand.NextSingle(minValue, maxValue),
                               rand.NextSingle(minValue, maxValue));
        }
        public static Vector3 NextVector3(this Random rand, float minValue, float maxValue)
        {
            return new Vector3(rand.NextSingle(minValue, maxValue),
                               rand.NextSingle(minValue, maxValue),
                               rand.NextSingle(minValue, maxValue));
        }
        public static Vector2 NextVector2(this Random rand, Vector2 minValue, Vector2 maxValue)
        {
            return new Vector2(rand.NextSingle(minValue.X, maxValue.X),
                               rand.NextSingle(minValue.Y, maxValue.Y));
        }
        public static Vector3 NextVector3(this Random rand, Vector3 minValue, Vector3 maxValue)
        {
            return new Vector3(rand.NextSingle(minValue.X, maxValue.X),
                               rand.NextSingle(minValue.Y, maxValue.Y),
                               rand.NextSingle(minValue.Z, maxValue.Z));
        }
        public static Vector2 NextVector2Sphere(this Random rand)
        {
            Vector2 v;
            do
            {
                v = rand.NextVector2Box();
            }
            while (v.LengthSquared() > 1f);
            return v;
        }
        public static Vector3 NextVector3Sphere(this Random rand)
        {
            Vector3 v;
            do
            {
                v = rand.NextVector3Box();
            }
            while (v.LengthSquared() > 1f);
            return v;
        }
    }
}
