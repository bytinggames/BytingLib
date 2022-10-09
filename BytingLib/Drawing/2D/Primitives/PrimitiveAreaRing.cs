using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BytingLib
{
    public class PrimitiveAreaRing
    {
        public IList<Vector2> Vertices { get; set; }

        public PrimitiveAreaRing(PrimitiveLineRing ring, float thickness, float anchor = 0f)
        {
            Vertices = new Vector2[ring.Vertices.Count * 2];

            float t = thickness / 2f;

            for (int i = 0; i < ring.Vertices.Count; i++)
            {
                Vector2 a = ring.Vertices[i] - ring.Vertices[(i + ring.Vertices.Count - 1) % ring.Vertices.Count];
                Vector2 b = ring.Vertices[i] - ring.Vertices[(i + 1) % ring.Vertices.Count];
                float angleA = MathF.Atan2(a.Y, a.X);
                float angleB = MathF.Atan2(b.Y, b.X);
                float angleDistHalved = MathExtension.AngleDistance(angleA, angleB) / 2f;
                float sin = MathF.Sin(angleDistHalved);
                float x = t / sin;
                float angleToCorner = angleA + angleDistHalved;
                Vector2 dirToCorner = new Vector2(MathF.Cos(angleToCorner), MathF.Sin(angleToCorner)) * x;

                Vertices[i * 2] = ring.Vertices[i] + dirToCorner * (1f - anchor);
                Vertices[i * 2 + 1] = ring.Vertices[i] + dirToCorner * (-1f - anchor);
            }
        }
        public void Draw(GraphicsDevice gDevice)
        {
            var arr = GetDrawableVertexPositions();
            gDevice.DrawUserPrimitives<VertexPosition>(PrimitiveType.TriangleStrip, arr, 0, arr.Length - 2);
        }

        public void Draw(SpriteBatch spriteBatch, Color color)
            => Draw(spriteBatch, color, spriteBatch.DefaultDepth);

        public void Draw(SpriteBatch spriteBatch, Color color, float depth)
        {
            var arr = GetDrawableVertexVectors();
            spriteBatch.DrawStrip(spriteBatch.GetPixel(), arr.ToList(), color, depth);
        }

        public void DrawGradient(SpriteBatch spriteBatch, Color color1, Color color2)
            => DrawGradient(spriteBatch, color1, color2, spriteBatch.DefaultDepth);
        public void DrawGradient(SpriteBatch spriteBatch, Color color1, Color color2, float depth)
        {
            spriteBatch.DrawStrip(spriteBatch.GetPixel(),
                    GetDrawableVertexPositionsGradient(color1, color2, depth)
                );
        }

        public VertexPosition[] GetDrawableVertexPositions()
        {
            return Vertices.Select(f => new VertexPosition(new Vector3(f, 0f)))
                .Concat(new List<VertexPosition>()
                {
                    new VertexPosition(new Vector3(Vertices[0], 0f)),
                    new VertexPosition(new Vector3(Vertices[1], 0f))
                })
                .ToArray();
        }

        public VertexPositionColorTexture[] GetDrawableVertexPositionsGradient(Color color1, Color color2, float depth)
        {
            bool toggle = true;
            return GetDrawableVertexVectors()
                    .Select(f => new VertexPositionColorTexture(new Vector3(f.X, f.Y, depth), (toggle = !toggle) ? color1 : color2, Vector2.Zero))
                    .ToArray();
        }

        public IEnumerable<Vector2> GetDrawableVertexVectors()
        {
            return Vertices.Concat(Vertices.Take(2));
        }
    }
}
