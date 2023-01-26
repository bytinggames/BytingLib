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
                return extended;
            extended = new SpriteBatchExtended(spriteBatch);
            extendedData.Add(spriteBatch, extended);
            return extended;
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
