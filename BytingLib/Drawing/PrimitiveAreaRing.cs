using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BytingLib
{
    public class PrimitiveAreaRing
    {
        public Vector2[] Vertices { get; set; }

        public PrimitiveAreaRing(PrimitiveLineRing ring, float thickness, float anchor = 0f)
        {
            Vertices = new Vector2[ring.Vertices.Length * 2];

            float t = thickness / 2f;

            for (int i = 0; i < ring.Vertices.Length; i++)
            {
                Vector2 a = ring.Vertices[i] - ring.Vertices[(i + ring.Vertices.Length - 1) % ring.Vertices.Length];
                Vector2 b = ring.Vertices[i] - ring.Vertices[(i + 1) % ring.Vertices.Length];
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
            var arr = GetVertexPositions();
            gDevice.DrawUserPrimitives<VertexPosition>(PrimitiveType.TriangleStrip, arr, 0, arr.Length - 2);
        }

        public VertexPosition[] GetVertexPositions()
        {
            return Vertices.Select(f => new VertexPosition(new Vector3(f, 0f)))
                .Concat(new List<VertexPosition>()
                {
                    new VertexPosition(new Vector3(Vertices[0], 0f)),
                    new VertexPosition(new Vector3(Vertices[1], 0f))
                })
                .ToArray();
        }
    }
}
