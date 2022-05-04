

using Microsoft.Xna.Framework;

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
    }
}
