using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BytingLib
{
    public class PrimitiveLineStrip
    {
        public List<Vector2> Vertices { get; set; }
        /// <summary>Should be normalized.</summary>
        public Vector2 LastVectorDirection { get; set; } // used for concatenating arcs without losing the correct direction after an arc was added

        public PrimitiveLineStrip(params Vector2[] vertices)
        {
            this.Vertices = vertices.ToList();
        }

        /// <summary>Warning: the list isn't cloned.</summary>
        public PrimitiveLineStrip(List<Vector2> vertices)
        {
            this.Vertices = vertices;
        }

        //public PrimitiveLineStrip(PrimitiveAreaStrip area)
        //{
        //    if (area.Vertices.Length < 3)
        //        throw new Exception("TriangleStrip with less than 3 vertices is not allowed");

        //    Vertices = new Vector2[area.Vertices.Length];
        //    // start edge
        //    Vertices[0] = area.Vertices[0];
        //    Vertices[1] = area.Vertices[1];
        //    // bottom line
        //    int i;
        //    int j;
        //    for (i = 2, j = 3; j < area.Vertices.Length; i++, j += 2)
        //        Vertices[i] = area.Vertices[j];
        //    // top line
        //    for (j = area.Vertices.Length - 2; j > 0; i++, j -= 2)
        //        Vertices[i] = area.Vertices[j];
        //}

        public PrimitiveAreaStrip Thicken(float thickness)
        {
            return new PrimitiveAreaStrip(this, thickness);
        }
        public PrimitiveAreaStrip ThickenOutside(float thickness)
        {
            return new PrimitiveAreaStrip(this, thickness, 1f);
        }
        public PrimitiveAreaStrip ThickenInside(float thickness)
        {
            return new PrimitiveAreaStrip(this, thickness, -1f);
        }
        public PrimitiveAreaStrip Thicken(float thickness, float anchorInner)
        {
            return new PrimitiveAreaStrip(this, thickness, anchorInner);
        }

        public static PrimitiveLineStrip CreateArc(Vector2 center, float radius, float angleStart, float arc, int vertexCount)
        {
            Vector2[] vertices = new Vector2[vertexCount];
            for (int i = 0; i < vertexCount; i++)
            {
                float angle = angleStart + arc * i / vertexCount;
                vertices[i] = center + new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * radius;
            }
            return new PrimitiveLineStrip(vertices);
        }

        /// <param name="angle">Positive for clockwise, negative for counter-clockwise.</param>
        /// <param name="vertexCount">-1 for setting it automatically.</param>
        public PrimitiveLineStrip AddArc(float radius, float angle, int vertexCount = -1)
        {
            if (Vertices.Count == 0)
                throw new Exception("no vertices to start with");
            if (Vertices.Count < 2 && LastVectorDirection == Vector2.Zero)
                throw new Exception("no two vertices and no LastVectorDirection to start with");

            if (angle == 0)
                return this;

            Vector2 dir;
            if (LastVectorDirection == Vector2.Zero)
                dir = Vertices[Vertices.Count - 1] - Vertices[Vertices.Count - 2];
            else
                dir = LastVectorDirection;
            dir.Normalize();

            float dirAngle = MathF.Atan2(dir.Y, dir.X);
            float dirAngleAfterArc = dirAngle + angle;
            LastVectorDirection = new Vector2(MathF.Cos(dirAngleAfterArc), MathF.Sin(dirAngleAfterArc));

            if (radius == 0)
                return this;

            Vector2 dirN = new Vector2(-dir.Y, dir.X);
            if (angle < 0)
                dirN = -dirN;

            Vector2 circleCenter = Vertices[Vertices.Count - 1] + dirN * radius;
            float circleAngle = dirAngle;
            if (angle > 0)
                circleAngle -= MathHelper.PiOver2;
            else
                circleAngle += MathHelper.PiOver2;

            if (vertexCount == -1)
            {
                vertexCount = Math.Max(1, (int)(DrawHelper.RadiusToVertexCount(radius) * MathF.Abs(angle) / MathHelper.TwoPi));
            }

            for (int i = 0; i < vertexCount; i++)
            {
                float currentAngle = angle * (i + 1) / vertexCount;
                currentAngle += circleAngle;

                Vertices.Add(circleCenter + new Vector2(MathF.Cos(currentAngle), MathF.Sin(currentAngle)) * radius);
            }

            return this;
        }

        public PrimitiveLineStrip AddVertex(float relativeX, float relativeY)
        {
            Vertices.Add(Vertices[Vertices.Count - 1] + new Vector2(relativeX, relativeY));
            LastVectorDirection = Vector2.Zero; // reset, cause that direction can be read from the last two vertices
            return this;
        }
        public PrimitiveLineStrip AddVertex(Vector2 relativePosition)
        {
            Vertices.Add(Vertices[Vertices.Count - 1] + relativePosition);
            LastVectorDirection = Vector2.Zero; // reset, cause that direction can be read from the last two vertices
            return this;
        }
        public PrimitiveLineStrip AddVertexAbsolute(Vector2 position)
        {
            Vertices.Add(position);
            LastVectorDirection = Vector2.Zero; // reset, cause that direction can be read from the last two vertices
            return this;
        }
        public PrimitiveLineStrip AddVertexAbsolute(float x, float y)
        {
            Vertices.Add(new Vector2(x,y));
            LastVectorDirection = Vector2.Zero; // reset, cause that direction can be read from the last two vertices
            return this;
        }

        public PrimitiveLineStrip Transform(Matrix matrix)
        {
            for (int i = 0; i < Vertices.Count; i++)
            {
                Vertices[i] = Vector2.Transform(Vertices[i], matrix);
            }
            return this;
        }
    }
}
