namespace BytingLib
{
    public static class IntExtension
    {
        public static int Wrap(this int n, int wrapRange)
        {
            n %= wrapRange;
            if (n < 0)
            {
                n += wrapRange;
            }
            return n;
        }
    }
}
