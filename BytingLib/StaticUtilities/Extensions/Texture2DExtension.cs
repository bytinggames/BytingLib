using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BytingLib
{
    public static class Texture2DExtension
    {
        public static Texture2D GenerateOutline4Directions(this Texture2D source, GraphicsDevice gDevice, Color? color = null, bool mergeWithSource = false)
        {
            Color[] colorOut = GenerateOutline4Directions(source, color, mergeWithSource);

            Texture2D outline = new Texture2D(gDevice, source.Width, source.Height);
            outline.SetData(colorOut);
            return outline;
        }
        public static void GenerateOutline4DirectionsRef(this Texture2D source, GraphicsDevice gDevice, Color? color = null, bool mergeWithSource = true)
        {
            Color[] colorOut = GenerateOutline4Directions(source, color, mergeWithSource);

            source.SetData(colorOut);
        }

        public static Color[] GenerateOutline4Directions(this Texture2D source, Color? color = null, bool mergeWithSource = false)
        {
            Color[] colorOut = new Color[source.Width * source.Height];
            Color[] colorIn = new Color[source.Width * source.Height];
            source.GetData(colorIn);

            Color c = color ?? Color.White;

            Shift(colorIn, colorOut, source.Width, source.Height, 1, 0, c);
            Shift(colorIn, colorOut, source.Width, source.Height, -1, 0, c);
            Shift(colorIn, colorOut, source.Width, source.Height, 0, 1, c);
            Shift(colorIn, colorOut, source.Width, source.Height, 0, -1, c);

            if (mergeWithSource)
            {
                for (int i = 0; i < colorOut.Length; i++)
                {
                    if (colorIn[i].A != 0)
                        colorOut[i] = colorIn[i];
                }
            }

            return colorOut;
        }

        public static void GenerateOutline8DirectionsRef(this Texture2D source, GraphicsDevice gDevice, Color? color = null, bool mergeWithSource = true)
        {
            Color[] colorOut = GenerateOutline8Directions(source, color, mergeWithSource);

            source.SetData(colorOut);
        }

        public static Texture2D GenerateOutline8Directions(this Texture2D source, GraphicsDevice gDevice, Color? color = null, bool mergeWithSource = false)
        {
            Color[] colorOut = GenerateOutline8Directions(source, color, mergeWithSource);

            Texture2D outline = new Texture2D(gDevice, source.Width, source.Height);
            outline.SetData(colorOut);
            return outline;
        }

        private static Color[] GenerateOutline8Directions(Texture2D source, Color? color = null, bool mergeWithSource = false)
        {
            Color[] colorOut = new Color[source.Width * source.Height];
            Color[] colorIn = new Color[source.Width * source.Height];
            source.GetData(colorIn);

            Color c = color ?? Color.White;

            Shift(colorIn, colorOut, source.Width, source.Height, 1, 0, c);
            Shift(colorIn, colorOut, source.Width, source.Height, -1, 0, c);
            Shift(colorIn, colorOut, source.Width, source.Height, 0, 1, c);
            Shift(colorIn, colorOut, source.Width, source.Height, 0, -1, c);

            Shift(colorIn, colorOut, source.Width, source.Height, 1, 1, c);
            Shift(colorIn, colorOut, source.Width, source.Height, -1, 1, c);
            Shift(colorIn, colorOut, source.Width, source.Height, -1, -1, c);
            Shift(colorIn, colorOut, source.Width, source.Height, 1, -1, c);

            if (mergeWithSource)
            {
                for (int i = 0; i < colorOut.Length; i++)
                {
                    if (colorIn[i].A != 0)
                        colorOut[i] = colorIn[i];
                }
            }

            return colorOut;
        }

        public static Texture2D GenerateColorize(this Texture2D source, GraphicsDevice gDevice, Color? color = null)
        {
            Color myColor;
            if (color == null)
                myColor = Color.White;
            else
                myColor = color.Value;
            Color[] colors = new Color[source.Width * source.Height];
            source.GetData(colors);
            for (int i = 0; i < colors.Length; i++)
            {
                colors[i].R = (byte)((myColor.R * colors[i].A) / 255);
                colors[i].G = (byte)((myColor.G * colors[i].A) / 255);
                colors[i].B = (byte)((myColor.B * colors[i].A) / 255);
            }
            return colors.ToTexture(source.Width, gDevice);
        }

        private static void Shift(Color[] colorIn, Color[] colorOut, int w, int h, int shiftX, int shiftY, Color color)
        {
            int w2 = Math.Min(w, w - shiftX);
            int h2 = Math.Min(h, h - shiftY);
            for (int y = Math.Max(-shiftY, 0); y < h2; y++)
            {
                for (int x = Math.Max(-shiftX, 0); x < w2; x++)
                {
                    if (colorIn[y * w + x].A != 0 && colorIn[(y + shiftY) * w + (x + shiftX)].A == 0)
                        colorOut[(y + shiftY) * w + (x + shiftX)] = color;
                }
            }
        }

        public static Texture2D GenerateGlow(this Texture2D source, int glowThickness, GraphicsDevice gDevice, Color? glowColor = null, bool mergeWithSource = false)
        {
            Color[] colorIn = new Color[source.Width * source.Height];
            source.GetData(colorIn);
            return GenerateGlow(colorIn, source.Width, source.Height, glowThickness, gDevice, glowColor, mergeWithSource);
        }

        public static void GenerateGlowRef(this Texture2D source, int glowThickness, GraphicsDevice gDevice, Color? glowColor = null, bool mergeWithSource = true)
        {
            Color[] colorIn = new Color[source.Width * source.Height];
            source.GetData(colorIn);
            source.SetData(GenerateGlow(colorIn, source.Width, source.Height, glowThickness, glowColor, mergeWithSource));
        }

        public static Texture2D GenerateGlow(Color[] colorIn, int w, int h, int glowThickness, GraphicsDevice gDevice, Color? glowColor = null, bool mergeWithSource = false)
        {
            return GenerateGlow(colorIn, w, h, glowThickness, glowColor, mergeWithSource).ToTexture(w, gDevice);
        }
        public static Color[] GenerateGlow(Color[] colorIn, int w, int h, int glowThickness, Color? glowColor = null, bool mergeWithSource = false)
        {
            Color[] colorOut = new Color[w * h];

            Color color = glowColor ?? Color.White;

            for (int i = 0; i < glowThickness; i++)
            {
                Color cColor = color * ((float)(glowThickness - i) / glowThickness);
                // shift
                Shift(colorIn, colorOut, w, h, 1, 0, cColor);
                Shift(colorIn, colorOut, w, h, -1, 0, cColor);
                Shift(colorIn, colorOut, w, h, 0, 1, cColor);
                Shift(colorIn, colorOut, w, h, 0, -1, cColor);

                // apply glow to source color
                for (int j = 0; j < colorIn.Length; j++)
                {
                    if (colorOut[j].A != 0)
                        colorIn[j] = colorOut[j];
                }
            }

            if (mergeWithSource)
            {
                for (int i = 0; i < colorOut.Length; i++)
                {
                    if (colorIn[i].A != 0)
                        colorOut[i] = colorIn[i];
                }
            }

            return colorOut;
        }

        public static Color[] OverrideColorsBasic(params Texture2D[] texs)
        {
            Color[] output = new Color[texs[0].Width * texs[0].Height];

            for (int j = 0; j < texs.Length; j++)
            {
                Color[] input = texs[j].ToColor();
                for (int i = 0; i < output.Length; i++)
                {
                    if (input[i].A != 0)
                        output[i] = input[i];
                }
            }

            return output;
        }

        public static Color[] ToColor(this Texture2D tex)
        {
            Color[] colors = new Color[tex.Width * tex.Height];
            tex.GetData(colors);
            return colors;
        }

        public static Texture2D ToTexture(this Color[] colors, int w, GraphicsDevice gDevice)
        {
            Texture2D tex = new Texture2D(gDevice, w, colors.Length / w);
            tex.SetData(colors);
            return tex;
        }

        public static Vector2 GetSize(this Texture2D tex)
        {
            return new Vector2(tex.Width, tex.Height);
        }
        public static Int2 GetSizeInt(this Texture2D tex)
        {
            return new Int2(tex.Width, tex.Height);
        }

        public static Texture2D? BlendOver(this Texture2D tex1, Texture2D tex2, GraphicsDevice gDevice)
        {
            if (tex1.Width != tex2.Width || tex1.Height != tex2.Height)
                return null;

            Color[] colors = tex1.ToColor();
            Color[] colors2 = tex2.ToColor();
            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = colors[i].BlendOver(colors2[i]);
            }
            Texture2D tex = new Texture2D(gDevice, tex1.Width, tex1.Height);
            tex.SetData(colors);
            return tex;
        }

        public static void InvertRef(this Texture2D tex)
        {
            Color[] colors = tex.ToColor();
            for (int i = 0; i < colors.Length; i++)
            {
                if (colors[i].A != 0)
                {
                    colors[i].R = (byte)(255 - colors[i].R);
                    colors[i].G = (byte)(255 - colors[i].G);
                    colors[i].B = (byte)(255 - colors[i].B);
                }
            }
            tex.SetData(colors);
        }
        public static Texture2D InvertCopy(this Texture2D tex, GraphicsDevice gDevice)
        {
            Color[] colors = tex.ToColor();
            for (int i = 0; i < colors.Length; i++)
            {
                if (colors[i].A != 0)
                {
                    colors[i].R = (byte)(255 - colors[i].R);
                    colors[i].G = (byte)(255 - colors[i].G);
                    colors[i].B = (byte)(255 - colors[i].B);
                }
            }
            Texture2D texOut = new Texture2D(gDevice, tex.Width, tex.Height);
            tex.SetData(colors);
            return tex;
        }

        public static Color BlendOver(this Color c1, Color c2)
        {
            float a = c1.A / 255f;
            float aInv = 1f - a;
            c2.R = (byte)(c1.R * a + c2.R * aInv);
            c2.G = (byte)(c1.G * a + c2.G * aInv);
            c2.B = (byte)(c1.B * a + c2.B * aInv);
            c2.A = (byte)(c2.A + ((255f - c2.A) * (c1.A / 255f)));
            return c2;
        }

        public static Color[] Crop(this Color[] colors, int colorsW, Rectangle crop)
        {
            Color[] output = new Color[crop.Width * crop.Height];

            for (int i = 0; i < output.Length; i++)
            {
                int x = i % crop.Width;
                int y = i / crop.Width;
                x += crop.X;
                y += crop.Y;

                int j = y * colorsW + x;

                output[i] = colors[j];
            }
            return output;
        }

        #region Drawing

        public static void Draw(this Texture2D _texture, SpriteBatch spriteBatch, Vector2 _position, Color? _color = null, Rectangle? _sourceRectangle = null, Vector2? _scale = null, float _rotation = 0f, SpriteEffects _effects = SpriteEffects.None, float? depth = null)
        {
            spriteBatch.Draw(_texture, _position, _sourceRectangle, _color ?? Color.White, _rotation, Vector2.Zero, _scale ?? Vector2.One, _effects, depth ?? spriteBatch.DefaultDepth);
        }

        public static void Draw(this Texture2D _texture, SpriteBatch spriteBatch, Anchor _anchor, Color? _color = null, Rectangle? _sourceRectangle = null, Vector2? _scale = null, float _rotation = 0f, SpriteEffects _effects = SpriteEffects.None, float? depth = null, float roundPositionTo = 0)
        {
            Vector2 size = _sourceRectangle != null ? _sourceRectangle.Value.Size.ToVector2() : _texture.GetSize();

            Vector2 pos = _anchor.pos;

            if (roundPositionTo != 0) // TODO: improve this correction, by calculating the anchor by yourself
            {
                if (_rotation == 0)
                {
                    Vector2 shift = _anchor.origin * size * (_scale ?? Vector2.One);

                    Vector2 drawPos = _anchor.pos - shift;

                    if (roundPositionTo == 1f)
                    {
                        drawPos = drawPos.RoundVectorCustom();
                    }
                    else
                    {
                        drawPos = (drawPos / roundPositionTo).RoundVectorCustom() * roundPositionTo;
                    }

                    pos = drawPos + shift;
                }
            }


            spriteBatch.Draw(_texture, pos, _sourceRectangle, _color ?? Color.White, _rotation, _anchor.origin * size, _scale ?? Vector2.One, _effects, depth ?? spriteBatch.DefaultDepth);
        }
        public static void Draw(this Texture2D _texture, SpriteBatch spriteBatch, Rectangle _rectangle, Color? _color = null, Rectangle? _sourceRectangle = null, float _rotation = 0f, SpriteEffects _effects = SpriteEffects.None, float? depth = null)
        {
            spriteBatch.Draw(_texture, _rectangle, _sourceRectangle, _color ?? Color.White, _rotation, Vector2.Zero, _effects, depth ?? spriteBatch.DefaultDepth);
        }

        public static void Draw(this Texture2D _texture, SpriteBatch spriteBatch, Rect _rectangle, Color? _color = null, Rectangle? _sourceRectangle = null, float _rotation = 0f, SpriteEffects _effects = SpriteEffects.None, float? depth = null)
        {
            Vector2 scale = _rectangle.Size / _texture.GetSize();
            _texture.Draw(spriteBatch, _rectangle.Pos, _color, _sourceRectangle, scale, _rotation, _effects, depth);
        }


        /// <summary>
        /// use this method, if you have many values either being x.5 or x.0, cause this will prevent unwanted floating point errors
        /// </summary>
        private static Vector2 RoundVectorCustom(this Vector2 vec)
        {
            return new Vector2((float)Math.Round(vec.X - 0.1f), (float)Math.Round(vec.Y - 0.1f));
        }

        #endregion
    }
}
