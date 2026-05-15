using System.Globalization;

namespace BytingLib
{
    public static class ColorExtension
    {
        private static readonly Vector2[] msaa8Kernel;

        static ColorExtension()
        {
            msaa8Kernel = GetMsaaKernel();
        }

        private static Vector2[] GetMsaaKernel()
        {
            Vector2[] kernel = [
                new(4,0),
                new(2,1),
                new(0,2),
                new(1,4),
                new(3,7),
                new(5,6),
                new(7,5),
                new(6,3)
            ];
            // map to range [-0.5;0.5]
            for (int i = 0; i < kernel.Length; i++)
            {
                kernel[i] = (kernel[i] - new Vector2(3.5f)) / 8f;
            }
            return kernel;
        }

        public static Color AddColors(Color c1, Color c2)
        {
            c1 *= (1f - (c2.A / 255f));
            c2 *= (c2.A / 255f);
            return new Color(c1.R + c2.R, c1.G + c2.G, c1.B + c2.B, c1.A + c2.A);
        }
        public static Color OverlayColors(Color c1, Color c2)
        {
            return new Color(c1.R + c2.R, c1.G + c2.G, c1.B + c2.B, Math.Min(byte.MaxValue, c1.A + c2.A));
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
            {
                hsv.hue = 0;
            }
            else if (cmax == r)
            {
                hsv.hue = 60f * ((g - b) / dist % 6);
            }
            else if (cmax == g)
            {
                hsv.hue = 60 * ((b - r) / dist + 2);
            }
            else if (cmax == b)
            {
                hsv.hue = 60 * ((r - g) / dist + 4);
            }
            else
            { }

            if (cmax == 0)
            {
                hsv.saturation = 0;
            }
            else
            {
                hsv.saturation = dist / cmax;
            }

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
            {
                if (colors[i].A > 0)
                {
                    colors[i] = color;
                }
            }

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
            {
                if (colors[i].A > 0)
                {
                    colors[i] = color;
                }
            }

            tex = new Texture2D(tex.GraphicsDevice, tex.Width, tex.Height);

            tex.SetData<Color>(colors);

            return tex;
        }

        public static Color FromHex(string hex)
        {
            return new Color(hex);
        }
        public static Color FromHex(int hex)
        {
            int r = (hex & 0xff0000) >> 16;
            int g = (hex & 0x00ff00) >> 8;
            int b = hex & 0x0000ff;

            return new Color(r, g, b);
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

        public static void LinearToSrgb(float[] color)
        {
            for (int i = 0; i < color.Length; i++)
            {
                color[i] = (color[i] <= 0.0031308f) ? 12.92f * color[i] : (1f + 0.055f) * MathF.Pow(color[i], 1f / 2.4f) - 0.055f;
            }
        }

        /// <summary>Needs to be tested</summary>
        public static void SrgbToLinear(float[] color)
        {
            for (int i = 0; i < color.Length; i++)
            {
                color[i] = (color[i] > 0.04045f) ? MathF.Pow((color[i] + 0.055f) / (1f + 0.055f), 2.4f) : color[i] / 12.92f;
            }
        }

        /// <summary>Can be optimized</summary>
        public static void ShrinkImage(Color[,] source, Color[,] destination, int shrinkBy)
        {
            int shrinkBySquared = shrinkBy * shrinkBy;

            int w = destination.GetLength(0);
            int h = destination.GetLength(1);
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    int r, g, b, a;
                    r = g = b = a = 0;
                    for (int y1 = 0; y1 < shrinkBy; y1++)
                    {
                        for (int x1 = 0; x1 < shrinkBy; x1++)
                        {
                            r += source[x * shrinkBy + x1, y * shrinkBy + y1].R;
                            g += source[x * shrinkBy + x1, y * shrinkBy + y1].G;
                            b += source[x * shrinkBy + x1, y * shrinkBy + y1].B;
                            a += source[x * shrinkBy + x1, y * shrinkBy + y1].A;
                        }
                    }
                    destination[x, y] = new Color(r / shrinkBySquared, g / shrinkBySquared, b / shrinkBySquared, a / shrinkBySquared);
                }
            }
        }

        /// <summary>Can be optimized</summary>
        public static void ShrinkImage(Vector4[,] source, Vector4[,] destination, int shrinkBy)
        {
            int shrinkBySquared = shrinkBy * shrinkBy;

            int w = destination.GetLength(0);
            int h = destination.GetLength(1);
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    float r, g, b, a;
                    r = g = b = a = 0;
                    for (int y1 = 0; y1 < shrinkBy; y1++)
                    {
                        for (int x1 = 0; x1 < shrinkBy; x1++)
                        {
                            r += source[x * shrinkBy + x1, y * shrinkBy + y1].X;
                            g += source[x * shrinkBy + x1, y * shrinkBy + y1].Y;
                            b += source[x * shrinkBy + x1, y * shrinkBy + y1].Z;
                            a += source[x * shrinkBy + x1, y * shrinkBy + y1].W;
                        }
                    }
                    destination[x, y] = new Vector4(r / shrinkBySquared, g / shrinkBySquared, b / shrinkBySquared, a / shrinkBySquared);
                }
            }
        }

        public static void ScaleToTargetSize(ref Color[] input, int inputWidth, ref Color[] output, int outputWidth, bool keepAspectRatio)
        {
            int inputHeight = input.Length / inputWidth;
            int outputHeight = output.Length / outputWidth;

            Rect renderRect = new Rect(0, 0, outputWidth, outputHeight);
            if (keepAspectRatio)
            {
                float inputAspect = (float)inputWidth / inputHeight;
                renderRect.ShrinkToAspectRatio(inputAspect, new Vector2(0.5f));
            }

            int left = (int)renderRect.Left;
            int top = (int)renderRect.Top;
            int right = (int)renderRect.Right;
            int bottom = (int)renderRect.Bottom;

            const int msaa = 8; // msut be equal to length of msaa8Kernel
            Vector2 pixelSizeOnInput = new Vector2(1f / renderRect.Width, 1f / renderRect.Height);

            if (bottom >= outputHeight)
            {
                bottom = outputHeight - 1;
            }
            if (right >= outputWidth)
            {
                right = outputWidth - 1;
            }

            Vector2 texCoord = Vector2.Zero;
            for (int xOutput = left, xRender = 0; xOutput <= right; xOutput++, xRender++)
            {
                for (int yOutput = top, yRender = 0; yOutput <= bottom; yOutput++, yRender++)
                {
                    // half pixel offset so (int) rounds to correct pixel
                    texCoord.X = (xRender + 0.5f) / renderRect.Width;
                    texCoord.Y = (yRender + 0.5f) / renderRect.Height;

                    Vector4 color = Vector4.Zero;
                    for (int i = 0; i < msaa; i++)
                    {
                        Vector2 texCoordMsaa = texCoord + pixelSizeOnInput * msaa8Kernel[i];
                        int sourceX = (int)(texCoordMsaa.X * inputWidth);
                        int sourceY = (int)(texCoordMsaa.Y * inputHeight);

                        if (sourceX >= 0 && sourceX < inputWidth
                            && sourceY >= 0 && sourceY < inputHeight)
                        {
                            int sourceIndex = sourceX + sourceY * inputWidth;
                            Vector4 c = input[sourceIndex].ToVector4();
                            // make sure transparent pixels contribute less
                            c.X *= c.W;
                            c.Y *= c.W;
                            c.Z *= c.W;
                            color += c;
                        }
                    }
                    color /= msaa; // normalize
                    // restore previous alpha
                    color.X /= color.W;
                    color.Y /= color.W;
                    color.Z /= color.W;

                    output[xOutput + yOutput * outputWidth] = new Color(color);
                }
            }
        }

        /// <summary>Can be optimized</summary>
        public static void BytesToColors(byte[] bytes, Color[,] colors)
        {
            int w = colors.GetLength(0);
            int h = colors.GetLength(1);

            if (w * h * 4 < bytes.Length)
            {
                throw new Exception("colors array size is too small for bytes array");
            }

            int i = 0;
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    colors[x, y] = new Color(bytes[i++],
                        bytes[i++],
                        bytes[i++],
                        bytes[i++]
                    );
                }
            }
        }

        /// <summary>Can be optimized</summary>
        public static void BytesToColors(byte[] bytes, Vector4[,] colors)
        {
            int w = colors.GetLength(0);
            int h = colors.GetLength(1);

            if (w * h * 4 < bytes.Length)
            {
                throw new Exception("colors array size is too small for bytes array");
            }

            int i = 0;
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    colors[x, y] = new Vector4(bytes[i++] / 255f,
                        bytes[i++] / 255f,
                        bytes[i++] / 255f,
                        bytes[i++] / 255f
                    );
                }
            }
        }

        /// <summary>Can be optimized</summary>
        public static void BytesToColors(byte[] bytes, Color[] colors)
        {
            if (colors.Length * 4 < bytes.Length)
            {
                throw new Exception("colors array size is too small for bytes array");
            }

            for (int i = 0; i < bytes.Length;)
            {
                colors[i / 4] = new Color(
                    bytes[i++],
                    bytes[i++],
                    bytes[i++],
                    bytes[i++]
                );
            }
        }

        /// <summary>Can be optimized</summary>
        public static void ColorsToBytes(Color[,] colors, byte[] bytes)
        {
            int w = colors.GetLength(0);
            int h = colors.GetLength(1);

            if (bytes.Length < w * h * 4)
            {
                throw new Exception("bytes array size is too small for colors array");
            }

            int i = 0;
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    bytes[i++] = colors[x, y].R;
                    bytes[i++] = colors[x, y].G;
                    bytes[i++] = colors[x, y].B;
                    bytes[i++] = colors[x, y].A;
                }
            }
        }

        /// <summary>Can be optimized</summary>
        public static void ColorsToBytes(ref Color[] colors, ref byte[] bytes)
        {
            if (bytes.Length < colors.Length * 4)
            {
                throw new Exception("bytes array size is too small for colors array");
            }

            int i = 0;
            for (int j = 0; j < colors.Length; j++)
            {
                bytes[i++] = colors[j].R;
                bytes[i++] = colors[j].G;
                bytes[i++] = colors[j].B;
                bytes[i++] = colors[j].A;
            }
        }

        /// <summary>Can be optimized</summary>
        public static void ColorsToBytes(Vector4[,] colors, byte[] bytes)
        {
            int w = colors.GetLength(0);
            int h = colors.GetLength(1);

            if (bytes.Length < w * h * 4)
            {
                throw new Exception("bytes array size is too small for colors array");
            }

            int i = 0;
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    bytes[i++] = (byte)Math.Clamp(colors[x, y].X * 255, 0, 255);
                    bytes[i++] = (byte)Math.Clamp(colors[x, y].Y * 255, 0, 255);
                    bytes[i++] = (byte)Math.Clamp(colors[x, y].Z * 255, 0, 255);
                    bytes[i++] = (byte)Math.Clamp(colors[x, y].W * 255, 0, 255);
                }
            }
        }

        public static Color ToGrayscale(this Color color)
        {
            byte gray = (byte)(0.2126f * color.R + 0.7152f * color.G + 0.0722f * color.B);
            color.R = color.G = color.B = gray;
            return color;
        }
    }
}