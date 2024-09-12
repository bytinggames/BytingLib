﻿namespace BytingLib
{
    public class PrimitiveLineRing
    {
        public IList<Vector2> Vertices { get; set; }

        public PrimitiveLineRing(IList<Vector2> vertices)
        {
            this.Vertices = vertices;
        }

        public PrimitiveLineRing(PrimitiveAreaStrip area)
        {
            if (area.Vertices.Length < 3)
            {
                throw new Exception("TriangleStrip with less than 3 vertices is not allowed");
            }

            Vertices = new Vector2[area.Vertices.Length];
            // start edge
            Vertices[0] = area.Vertices[0];
            Vertices[1] = area.Vertices[1];
            // bottom line
            int i;
            int j;
            for (i = 2, j = 3; j < area.Vertices.Length; i++, j += 2)
            {
                Vertices[i] = area.Vertices[j];
            }
            // top line
            for (j = area.Vertices.Length - 2; j > 0; i++, j -= 2)
            {
                Vertices[i] = area.Vertices[j];
            }
        }

        public PrimitiveLineRing(Circle circle, int vertexCount)
        {
            Vertices = new Vector2[vertexCount];
            for (int i = 0; i < vertexCount; i++)
            {
                float angle = MathHelper.TwoPi * i / vertexCount;
                Vertices[i] = circle.Pos + new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * circle.Radius;
            }
        }

        public PrimitiveLineRing(Rect rect)
        {
            Vertices =
            [
                rect.BottomRight,
                rect.BottomLeft,
                rect.TopLeft,
                rect.TopRight,
            ];
        }

        public PrimitiveAreaRing Thicken(float thickness)
        {
            return new PrimitiveAreaRing(this, thickness);
        }
        public PrimitiveAreaRing ThickenOutside(float thickness)
        {
            return new PrimitiveAreaRing(this, thickness, 1f);
        }
        public PrimitiveAreaRing ThickenInside(float thickness)
        {
            return new PrimitiveAreaRing(this, thickness, -1f);
        }
        public PrimitiveAreaRing Thicken(float thickness, float anchorInner)
        {
            return new PrimitiveAreaRing(this, thickness, anchorInner);
        }

        public PrimitiveLineRing SkewX(float angle)
        {
            Polygon.SkewX(Vertices, angle);
            return this;
        }
        public PrimitiveLineRing SkewY(float angle)
        {
            Polygon.SkewY(Vertices, angle);
            return this;
        }

        public Polygon ToPolygon()
        {
            return new Polygon(Vector2.Zero, Vertices);
        }
    }
}
