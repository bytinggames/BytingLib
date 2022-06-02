using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;

namespace BytingLib
{
    public class PrimitiveAreaFan : PrimitiveArea
    {
        public Vector2[] Vertices { get; set; }

        public PrimitiveAreaFan(Vector2[] vertices)
        {
            this.Vertices = vertices;
        }

        public PrimitiveAreaFan(Circle circle, int vertexCount)
        {
            Vertices = new Vector2[vertexCount];

            for (int i = 0; i < vertexCount; i++)
            {
                float angle = MathHelper.TwoPi * i / vertexCount;
                Vertices[i] = circle.Pos + new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * circle.Radius;
            }
        }

        public override PrimitiveLineRing Outline()
        {
            return new PrimitiveLineRing(Vertices);
        }

        public override void Draw(GraphicsDevice gDevice)
        {
            var v = Vertices.Select(f => new VertexPosition(new Vector3(f, 0f))).ToArray();
            short[] indices = new short[(Vertices.Length - 1) * 3];
            short triCount = (short)(indices.Length / 3);
            short i;
            for (i = 0; i < triCount - 1; i++)
            {
                indices[i * 3 + 1] = (short)(i + 1);
                indices[i * 3 + 2] = (short)(i + 2);
            }
            indices[i * 3 + 1] = (short)(i + 1);
            indices[i * 3 + 2] = 1;

            gDevice.DrawUserIndexedPrimitives<VertexPosition>(PrimitiveType.TriangleList, v, 0, v.Length, indices, 0, triCount);
        }

        public override void Draw(SpriteBatch spriteBatch, Color color)
        {
            spriteBatch.DrawPolygon(spriteBatch.GetPixel(), Vertices, color);
        }
        public override void Draw(SpriteBatch spriteBatch, Color color, float layerDepth)
        {
            spriteBatch.DrawPolygon(spriteBatch.GetPixel(), Vertices, color, layerDepth);
        }

        public PrimitiveAreaFan Enlarge(float enlarge)
        {
            if (enlarge < 0)
                throw new ArgumentException(nameof(enlarge) + " < 0 is not supported");
            if (enlarge == 0)
                return this;

            var vertices = Outline().ThickenOutside(enlarge).Vertices;

            for (int i = 0; i < Vertices.Length; i++)
            {
                Vertices[i] = vertices[i * 2 + 1];
            }

            return this;
        }
    }
}
