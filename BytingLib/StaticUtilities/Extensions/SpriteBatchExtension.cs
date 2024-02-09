namespace BytingLib
{
    public static class SpriteBatchExtension
    {
        private static Dictionary<SpriteBatch, SpriteBatchExtended> extendedData = new Dictionary<SpriteBatch, SpriteBatchExtended>();

        public static void DrawRectangle(this SpriteBatch spriteBatch, Rect rect, Color color)
            => DrawRectangle(spriteBatch, rect, color, spriteBatch.DefaultDepth);
        public static void DrawRectangle(this SpriteBatch spriteBatch, Rect rect, Color color, float depth)
        {
            spriteBatch.DrawQuad(GetPixel(spriteBatch), rect.TopLeft, rect.TopRight, rect.BottomLeft, rect.BottomRight, color, depth);
        }

        public static void DrawPolygon(this SpriteBatch spriteBatch, Polygon polygon, Color color)
            => DrawPolygon(spriteBatch, polygon, color, spriteBatch.DefaultDepth);
        public static void DrawPolygon(this SpriteBatch spriteBatch, Polygon polygon, Color color, float depth)
        {
            spriteBatch.DrawPolygon(GetPixel(spriteBatch), polygon.Vertices, color, polygon.Pos, depth);
        }

        public static void DrawCircle(this SpriteBatch spriteBatch, Circle circle, Color color)
            => DrawCircle(spriteBatch, circle, color, spriteBatch.DefaultDepth);
        public static void DrawCircle(this SpriteBatch spriteBatch, Circle circle, Color color, float depth)
        {
            var polygon = circle.ToPolygon(GetExtended(spriteBatch).RadiusToVertexCount(circle.Radius));
            spriteBatch.DrawPolygon(polygon, color, depth);
        }
        public static void DrawCircleGradient(this SpriteBatch spriteBatch, Circle circle, Color colorInner, Color colorOuter)
            => DrawCircleGradient(spriteBatch, circle, colorInner, colorOuter, spriteBatch.DefaultDepth);
        public static void DrawCircleGradient(this SpriteBatch spriteBatch, Circle circle, Color colorInner, Color colorOuter, float depth)
        {
            var polygon = circle.ToPolygon(GetExtended(spriteBatch).RadiusToVertexCount(circle.Radius));
            List<VertexPositionColorTexture> v = polygon.Vertices
                .Select(f => new VertexPositionColorTexture(new Vector3(polygon.X + f.X, polygon.Y + f.Y, depth), colorOuter, Vector2.Zero))
                .ToList();
            v.Add(v[0]);
            v.Insert(0, new VertexPositionColorTexture(new Vector3(circle.X, circle.Y, depth), colorInner, Vector2.Zero));

            spriteBatch.DrawPolygon(spriteBatch.GetPixel(), v);
        }

        public static void DrawCone(this SpriteBatch spriteBatch, Vector2 origin, float radius, float angleStart, float angle, Color colorInner, Color colorOuter)
            => DrawCone(spriteBatch, origin, radius, angleStart, angle, colorInner, colorOuter, spriteBatch.DefaultDepth);
        public static void DrawCone(this SpriteBatch spriteBatch, Vector2 origin, float radius, float angleStart, float angle, Color colorInner, Color colorOuter, float depth)
        {
            var polygon = Polygon.GetCone(origin, radius, angleStart, angle, (int)(GetExtended(spriteBatch).RadiusToVertexCount(radius) * angle / MathHelper.TwoPi));
            List<VertexPositionColorTexture> v = polygon.Vertices
                .Select(f => new VertexPositionColorTexture(new Vector3(polygon.X + f.X, polygon.Y + f.Y, depth), colorOuter, Vector2.Zero))
                .ToList();
            v.Insert(0, new VertexPositionColorTexture(new Vector3(origin.X, origin.Y, depth), colorInner, Vector2.Zero));

            spriteBatch.DrawPolygon(spriteBatch.GetPixel(), v);
        }

        public static void DrawCross(this SpriteBatch spriteBatch, Vector2 pos, float diameter, float thickness, Color color)
            => DrawCross(spriteBatch, pos, diameter, thickness, color, spriteBatch.DefaultDepth);
        public static void DrawCross(this SpriteBatch spriteBatch, Vector2 pos, float diameter, float thickness, Color color, float depth)
        {
            Vector2 dir = Vector2.Normalize(Vector2.One);
            Vector2 diameterV = dir * diameter;
            Vector2 thicknessV = dir * thickness;
            thicknessV = thicknessV.GetRotate90();
            Vector2 corner = pos - thicknessV / 2f - diameterV / 2f;

            spriteBatch.DrawQuad(GetPixel(spriteBatch), corner, corner + diameterV, corner + thicknessV, corner + diameterV + thicknessV, color, depth);

            diameterV = diameterV.GetRotate90();
            thicknessV = thicknessV.GetRotate90();
            corner = pos - thicknessV / 2f - diameterV / 2f;
            spriteBatch.DrawQuad(GetPixel(spriteBatch), corner, corner + diameterV, corner + thicknessV, corner + diameterV + thicknessV, color, depth);
        }

        public static void DrawLine(this SpriteBatch spriteBatch, Vector2 pos1, Vector2 pos2, Color color, float thickness = 1f)
            => DrawLine(spriteBatch, pos1, pos2, color, spriteBatch.DefaultDepth, thickness);
        public static void DrawLine(this SpriteBatch spriteBatch, Vector2 pos1, Vector2 pos2, Color color, float depth, float thickness = 1f) => spriteBatch.DrawLineRelative(pos1, pos2 - pos1, color, depth, thickness);
        public static void DrawLineRelative(this SpriteBatch spriteBatch, Vector2 pos, Vector2 size, Color color, float thickness = 1f)
            => DrawLineRelative(spriteBatch, pos, size, color, spriteBatch.DefaultDepth, thickness);
        public static void DrawLineRelative(this SpriteBatch spriteBatch, Vector2 pos, Vector2 size, Color color, float depth, float thickness = 1f)
        {
            float angle = (float)Math.Atan2(size.Y, size.X);
            spriteBatch.Draw(GetPixel(spriteBatch), pos, null, color, angle, new Vector2(0, 0.5f), new Vector2(size.Length(), thickness), SpriteEffects.None, depth);
        }

        public static void DrawLineRounded(this SpriteBatch spriteBatch, Vector2 pos1, Vector2 pos2, Color color, float thickness = 1f)
            => DrawLineRounded(spriteBatch, pos1, pos2, color, spriteBatch.DefaultDepth, thickness);
        public static void DrawLineRounded(this SpriteBatch spriteBatch, Vector2 pos1, Vector2 pos2, Color color, float depth, float thickness = 1f)
        {
            Vector2 dist = pos2 - pos1;
            float dirAngle = MathF.Atan2(dist.Y, dist.X);
            float radius = thickness / 2f;
            spriteBatch.DrawCone(pos1, radius, dirAngle + MathHelper.PiOver2, MathHelper.Pi, color, color, depth);
            spriteBatch.DrawCone(pos2, radius, dirAngle - MathHelper.PiOver2, MathHelper.Pi, color, color, depth);
            DrawLine(spriteBatch, pos1, pos2, color, thickness);
        }

        public static Texture2D GetPixel(this SpriteBatch spriteBatch)
        {
            return GetExtended(spriteBatch).PixelTex;
        }

        private static SpriteBatchExtended GetExtended(SpriteBatch spriteBatch)
        {
            SpriteBatchExtended? extended;
            if (extendedData.TryGetValue(spriteBatch, out extended))
            {
                return extended;
            }

            extended = new SpriteBatchExtended(spriteBatch);
            extendedData.Add(spriteBatch, extended);
            return extended;
        }

        /// <summary>
        /// Submit a text string of sprites for drawing in the current batch.
        /// </summary>
        /// <remarks>
        /// This method's performance could be improved, by caching the line widths. So it should primarily be used for debugging or for constantly changing text.
        /// </remarks>
        /// <param name="spriteFont">A font.</param>
        /// <param name="text">The text which will be drawn.</param>
        /// <param name="position">The drawing location on screen.</param>
        /// <param name="color">A color mask.</param>
        /// <param name="rotation">A rotation of this string.</param>
        /// <param name="origin">Center of the rotation. 0,0 by default.</param>
        /// <param name="scale">A scaling of this string.</param>
        /// <param name="effects">Modificators for drawing. Can be combined.</param>
        /// <param name="layerDepth">A depth of the layer of this string.</param>
        /// <param name="rtl">Text is Right to Left.</param>
        /// <param name="horizontalAlign01">Horizontal alignment of the text. 0: left; 1: right</param>
        public static void DrawString(this SpriteBatch spriteBatch,
            SpriteFont spriteFont, string text, Vector2 position, Color color,
            float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth, bool rtl, float horizontalAlign01)
        {
            if (horizontalAlign01 == 0)
            {
                spriteBatch.DrawString(spriteFont, text, position, color, rotation, origin, scale, effects, layerDepth, rtl);
                return;
            }

            string[] lines = text.Split(new char[] { '\n' });
            float[] lineWidth = new float[lines.Length];
            float maxLineWidth = 0f;
            for (int i = 0; i < lines.Length; i++)
            {
                lineWidth[i] = spriteFont.MeasureString(lines[i]).X;
                maxLineWidth = MathF.Max(maxLineWidth, lineWidth[i]);
            }

            Vector2 offset = Vector2.Zero;
            for (int i = 0; i < lines.Length; i++)
            {
                offset.X = (maxLineWidth - lineWidth[i]) * horizontalAlign01;

                if (!string.IsNullOrEmpty(lines[i]))
                {
                    spriteBatch.DrawString(spriteFont, lines[i], position + offset, color, rotation, origin, scale, effects, layerDepth, rtl);
                }

                offset.Y += spriteFont.LineSpacing * scale.Y;
            }
        }
    }

    internal class SpriteBatchExtended
    {
        public Texture2D PixelTex { get; }
        public Func<float, int> RadiusToVertexCount { get; }

        public SpriteBatchExtended(SpriteBatch spriteBatch)
        {
            PixelTex = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
            PixelTex.SetData(new Color[] { Color.White });

            RadiusToVertexCount = DrawHelper.RadiusToVertexCount;

            spriteBatch.Disposing += SpriteBatch_Disposing;
        }

        private void SpriteBatch_Disposing(object? sender, EventArgs e)
        {
            PixelTex.Dispose();
        }
    }
}
