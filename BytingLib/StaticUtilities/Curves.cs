namespace BytingLib
{
    public static class Curves
    {
        // source: https://easings.net/

        public static float EaseInOutSine(float x)
        {
            return -(MathF.Cos(MathHelper.Pi * x) - 1f) / 2f;
        }
        public static float EaseInOutQuad(float x)
        {
            return x < 0.5f ? 2f * x * x : 1f - MathF.Pow(-2f * x + 2f, 2f) / 2f;
        }
        public static float EaseInOutCubic(float x)
        {
            return x < 0.5f ? 4f * x * x * x : 1f - MathF.Pow(-2f * x + 2f, 3f) / 2f;
        }

        public static float EaseOutSine(float x)
        {
            return MathF.Sin(x * MathHelper.Pi / 2f);
        }
        public static float EaseOutQuad(float x)
        {
            return 1f - (1f - x) * (1f - x);
        }
        public static float EaseOutCubic(float x)
        {
            return 1f - MathF.Pow(1f - x, 3f);
        }
        public static float EaseOutPow(float x, float exp)
        {
            return 1f - MathF.Pow(1f - x, exp);
        }

        public static float EaseInSine(float x)
        {
            return 1f - MathF.Cos(x * MathF.PI / 2f);
        }
        public static float EaseInQuad(float x)
        {
            return x * x;
        }
        public static float EaseInCubic(float x)
        {
            return x * x * x;
        }
        public static float EaseInPow(float x, float exp)
        {
            return MathF.Pow(x, exp);
        }

        public static float Linear(float x) => x;
        public static float LinearReverse(float x) => 1f - x;
        public static float One(float _) => 1f;
        public static float Zero(float _) => 0f;

        public static float Multiple(int frame, params (Func<float, float> func, int duration)[] funcs)
        {
            for (int i = 0; i < funcs.Length; i++)
            {
                if (frame <= funcs[i].duration)
                {
                    return funcs[i].func((float)frame / funcs[i].duration);
                }
                frame -= funcs[i].duration;
            }
            return funcs.Last().func(1f);
        }

        public static float Reverse(float x, Func<float, float> func)
        {
            return func(1f - x);
        }

        public static float BounceQuad(float x)
        {
            float pow = 2f * x - 1f;
            return 1f - pow * pow;
        }

        public static float BounceLinear(float x)
        {
            x %= 1f;
            x *= 2f;
            if (x > 1f)
            {
                return 2f - x;
            }
            else
            {
                return x;
            }
        }

        public static float BounceSine(float x)
        {
            return -MathF.Cos(x * MathHelper.TwoPi) * 0.5f + 0.5f;
        }

        public static float BouncePow(float x, float exponent)
        {
            x = BounceLinear(x);
            x *= 2f;
            x -= 1f;
            float sign = MathF.Sign(x);
            x = MathF.Pow(MathF.Abs(x), exponent);
            x *= sign;
            x += 1f;
            x /= 2f;
            return x;
        }

        public static float EaseOutElastic(float x)
        {
            const float c4 = 2f * MathF.PI / 3f;

            return x <= 0f
              ? 0f
              : x >= 1f
              ? 1f
              : MathF.Pow(2f, -10f * x) * MathF.Sin((x * 10f - 0.75f) * c4) + 1f;
        }

        public static float EaseMiddleSine(float x)
        {
            x = Math.Clamp(x, 0f, 1f);
            return 1f - MathF.Acos((x - 0.5f) * 2f) / MathHelper.Pi;
        }
        public static float EaseMiddlePow(float x, float exp)
        {
            return Split(x, x => EaseOutPow(x, exp), x => EaseInPow(x, exp));
        }

        public static float Split(float x, Func<float, float> toMiddle, Func<float, float> fromMiddle)
        {
            if (x < 0.5f)
            {
                x *= 2f;
                return toMiddle(x) * 0.5f;
            }
            else
            {
                x = (x - 0.5f) * 2f;
                return fromMiddle(x) * 0.5f + 0.5f;
            }
        }
    }
}
