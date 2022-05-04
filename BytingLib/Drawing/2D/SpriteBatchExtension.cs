using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BytingLib
{
    public static class SpriteBatchExtension
    {
        private static Dictionary<SpriteBatch, SpriteBatchExtended> extendedData = new Dictionary<SpriteBatch, SpriteBatchExtended>();

        public static void DrawRectangle(this SpriteBatch spriteBatch, Rect rect, Color color, float depth = 0f)
        {
            spriteBatch.DrawQuad(GetPixel(spriteBatch), rect.TopLeft, rect.TopRight, rect.BottomLeft, rect.BottomRight, color, depth);
        }

        public static void DrawPolygon(this SpriteBatch spriteBatch, Polygon polygon, Color color, float depth = 0f)
        {
            spriteBatch.DrawPolygon(GetPixel(spriteBatch), polygon.Pos, polygon.Vertices, color, depth);
        }

        public static void DrawCircle(this SpriteBatch spriteBatch, Circle circle, Color color, float depth = 0f)
        {
            var polygon = circle.ToPolygon(GetExtended(spriteBatch).RadiusToVertexCount(circle.Radius));
            spriteBatch.DrawPolygon(polygon, color, depth);
        }

        public static void DrawCross(this SpriteBatch spriteBatch, Vector2 pos, float diameter, float thickness, Color color, float depth = 0f)
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
