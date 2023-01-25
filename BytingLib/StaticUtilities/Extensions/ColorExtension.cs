using System.Globalization;

namespace BytingLib
{
    public static class ColorExtension
    {
        public static Color AddColors(Color c1, Color c2)
        {
            c1 *= (1f - (c2.A / 255f));
            c2 *= (c2.A / 255f);
            return new Color(c1.R + c2.R, c1.G + c2.G, c1.B + c2.B, c1.A + c2.A);
        }


        public static Color MultiplyColors(Color bottom, Color top)
        {
            bottom.R = (byte)(bottom.R * top.R / 255 * top.A / 255);
            bottom.G = (byte)(bottom.G * top.G / 255 * top.A / 255);
            bottom.B = (byte)(bottom.B * top.B / 255 * top.A / 255);
            bottom.A = (byte)(bottom.A * top.A / 255);
            return bottom;
        }

        public static Color AverageColor(Color c1, Color c2)
        {
            return new Color((c1.R + c2.R) / 2, (c1.G + c2.G) / 2, (c1.B + c2.B) / 2, (c1.A + c2.A) / 2);
        }
        public static Color AverageColor(params Color[] colors)
        {
            int r, g, b, a;
            r = g = b = a = 0;
            for (int i = 0; i < colors.Length; i++)
            {
                r += colors[i].R;
                g += colors[i].G;
                b += colors[i].B;
                a += colors[i].A;
            }
            int l = colors.Length;
            return new Color(r / l, g / l, b / l, a / l);
        }

        public static HSVColor ToHSV(this Color color)
        {
            float r = color.R / 255f;
            float g = color.G / 255f;
            float b = color.B / 255f;

            float cmax = Math.Max(r, Math.Max(g, b));
            float cmin = Math.Min(r, Math.Min(g, b));
            float dist = cmax - cmin;

            HSVColor hsv = new HSVColor();
            hsv.Alpha = color.A;

            if (dist == 0)
                hsv.hue = 0;
            else if (cmax == r)
                hsv.hue = 60f * ((g - b) / dist % 6);
            else if (cmax == g)
                hsv.hue = 60 * ((b - r) / dist + 2);
            else if (cmax == b)
                hsv.hue = 60 * ((r - g) / dist + 4);
            else
            { }

            if (cmax == 0)
                hsv.saturation = 0;
            else
                hsv.saturation = dist / cmax;

            hsv.value = cmax;

            return hsv;
        }

        /// <summary>
        /// warning: could crash on some graphics cards
        /// </summary>
        public static void ChangeTextureColors(Texture2D tex, int[] findHues, Color[] newColors)
        {
            Color[] colors = new Color[tex.Width * tex.Height];
            tex.GetData<Color>(colors);

            for (int i = 0; i < colors.Length; i++)
            {
                if (colors[i].A > 0)
                {
                    Color c = colors[i];
                    HSVColor hsv = ToHSV(colors[i]);

                    for (int j = 0; j < findHues.Length; j++)
                    {
                        if (Math.Round(hsv.hue) == findHues[j])
                        {
                            HSVColor hsvNew = ToHSV(newColors[j]);

                            hsv.saturation = 0;
                            hsv.hue = hsvNew.hue;
                            hsv.saturation = hsvNew.saturation;
                            hsv.value *= hsvNew.value;
                            colors[i] = hsv.ToRGB();
                            break;
                        }
                    }
                }
            }

            tex.SetData<Color>(colors);
        }

        /// <summary>
        /// warning: could crash on some graphics cards
        /// </summary>
        public static void FillTextureWithColor(Texture2D tex, Color color)
        {
            Color[] colors = new Color[tex.Width * tex.Height];
            tex.GetData<Color>(colors);

            for (int i = 0; i < colors.Length; i++)
                if (colors[i].A > 0)
                    colors[i] = color;
            tex.SetData<Color>(colors);
        }

        /// <summary>
        /// warning: could crash on some graphics cards
        /// </summary>
        public static Texture2D FillTextureWithColorClone(Texture2D tex, Color color)
        {
            Color[] colors = new Color[tex.Width * tex.Height];
            tex.GetData<Color>(colors);

            for (int i = 0; i < colors.Length; i++)
                if (colors[i].A > 0)
                    colors[i] = color;

            tex = new Texture2D(tex.GraphicsDevice, tex.Width, tex.Height);

            tex.SetData<Color>(colors);

            return tex;
        }

        static readonly CultureInfo hexToColorCultureInfo = new CultureInfo("en-GB");
        public static Color HexToColor(string hex)
        {
            if (string.IsNullOrEmpty(hex)
                || (hex.Length != 1 && hex.Length != 3 && hex.Length != 4 && hex.Length != 6 && hex.Length != 8))
                return Color.White;
            if (hex.Length == 1)
                hex = new string(hex[0], 6);
            else if (hex.Length == 3)
                hex = hex.Insert(0, hex[0].ToString()).Insert(2, hex[1].ToString()).Insert(4, hex[2].ToString());
            else if (hex.Length == 4)
                hex = hex[0].ToString() + hex[0] + hex[1] + hex[1] + hex[2] + hex[2] + hex[3] + hex[3];

            byte r, g, b, a = 255;
            byte.TryParse(hex.Substring(0, 2), NumberStyles.HexNumber, hexToColorCultureInfo, out r);
            byte.TryParse(hex.Substring(2, 2), NumberStyles.HexNumber, hexToColorCultureInfo, out g);
            byte.TryParse(hex.Substring(4, 2), NumberStyles.HexNumber, hexToColorCultureInfo, out b);

            if (hex.Length == 8)
                byte.TryParse(hex.Substring(6, 2), NumberStyles.HexNumber, hexToColorCultureInfo, out a);

            return new Color(r, g, b, a);
        }
        public static Color HexToColor(int hex)
        {
            int r = (hex & 0xff0000) >> 16;
            int g = (hex & 0x00ff00) >> 8;
            int b = hex & 0x0000ff;

            return new Color(r, g, b);
        }

        public static string ToHex(this Color color)
        {
            string hex = "";
            hex += color.R.ToString("X2");
            hex += color.G.ToString("X2");
            hex += color.B.ToString("X2");
            if (color.A != 255)
                hex += color.A.ToString("X2");
            return hex;
        }

        public static float GetColorBrightness(this Color color)
        {
            Vector3 v = color.ToVector3();
            return 0.2126f * v.X + 0.7152f * v.Y + 0.0722f * v.Z;
        }
        public static float GetColorBrightness(Vector3 color)
        {
            return 0.2126f * color.X + 0.7152f * color.Y + 0.0722f * color.Z;
        }

        public static Color GetChangeA(ref Color color, byte a)
        {
            color.A = a;
            return color;
        }
        public static Color GetChangeR(ref Color color, byte r)
        {
            color.R = r;
            return color;
        }
        public static Color GetChangeG(ref Color color, byte g)
        {
            color.G = g;
            return color;
        }
        public static Color GetChangeB(ref Color color, byte b)
        {
            color.B = b;
            return color;
        }

    }
}
