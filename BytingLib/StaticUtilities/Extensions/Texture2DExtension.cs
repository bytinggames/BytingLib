using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BytingLib
{
    public static class Texture2DExtension
    {
        public static Color[] ToColor(this Texture2D tex)
        {
            Color[] colors = new Color[tex.Width * tex.Height];
            tex.GetData(colors);
            return colors;
        }
    }
}
