﻿namespace BytingLib
{
    public class Polygon : IShape
    {
        private Vector2 pos;

        public IList<Vector2> Vertices;

        private byte _closed;

        public Polygon()
        {
            Vertices = new List<Vector2>();
            _closed = (byte)ClosedType.Closed;
        }
        public Polygon(Vector2 pos, IList<Vector2> vertices)
        {
            this.pos = pos;
            Vertices = vertices;
            _closed = (byte)ClosedType.Closed;
        }
        public Polygon(Vector2 pos, IList<Vector2> vertices, ClosedType closed)
        {
            this.pos = pos;
            Vertices = vertices;
            _closed = (byte)closed;
        }

        public Vector2 Pos { get => pos; set => pos = value; }
        public float X { get => pos.X; set => pos.X = value; }
        public float Y { get => pos.Y; set => pos.Y = value; }

        #region ClosedType

        /// <summary>
        /// Used for collision checks: the last edge between the last and first vertex can be included or excluded from collision checks.
        /// The same goes for the first and last corner/vertex.
        /// Only "Closed" is fully supported though.
        /// The other ones only are regarded, when a collision check between two open polygons is made or an open polygon and a circle
        /// (not tested though, it's been a long time).
        /// </summary>
        public enum ClosedType : byte
        {
            // 0b edge startCorner endCorner
            Closed = 0b111,
            OpenWithoutCorners = 0b000,
            OpenWithStartCorner = 0b010,
            OpenWithEndCorner = 0b001,
            OpenWithStartAndEndCorners = 0b011
        }

        /// <inheritdoc cref="ClosedType"/>
        public ClosedType Closed
        {
            get => (ClosedType)_closed;
            set => _closed = (byte)value;
        }
        /// <inheritdoc cref="ClosedType"/>
        public bool LastEdgeClosed => (_closed & 0b100) == 0b100;
        /// <inheritdoc cref="ClosedType"/>
        public bool StartCorner => (_closed & 0b010) == 0b010;
        /// <inheritdoc cref="ClosedType"/>
        public bool EndCorner => (_closed & 0b001) == 0b001;

        #endregion

        public Polygon ApplyPosition()
        {
            for (int i = 0; i < Vertices.Count; i++)
            {
                Vertices[i] += pos;
            }
            pos = Vector2.Zero;
            return this;
        }

        public Rect GetBoundingRect()
        {
            Vector2 minPos, maxPos;
            if (Vertices.Count > 0)
            {
                minPos = maxPos = Vertices[0];
                for (int i = 1; i < Vertices.Count; i++)
                {
                    minPos = Vector2.Min(minPos, Vertices[i]);
                    maxPos = Vector2.Max(maxPos, Vertices[i]);
                }

                return new Rect(pos + minPos, maxPos - minPos);
            }
            else
            {
                return new Rect(pos, Vector2.Zero);
            }
        }

        public List<Vector2> GetEdges()
        {
            List<Vector2> edges = new List<Vector2>();
            if (Vertices.Count > 1)
            {
                for (int i = 0; i < Vertices.Count - 1; i++)
                {
                    edges.Add(Vertices[i + 1] - Vertices[i]);
                }

                //if (vertices.Count > 2)
                if (LastEdgeClosed)
                {
                    edges.Add(Vertices[0] - Vertices[Vertices.Count - 1]);
                }
            }
            return edges;
        }

        internal List<Vector2> GetClosedEdges()
        {
            List<Vector2> edges = new List<Vector2>();
            if (Vertices.Count > 1)
            {
                for (int i = 0; i < Vertices.Count - 1; i++)
                {
                    edges.Add(Vertices[i + 1] - Vertices[i]);
                }

                edges.Add(Vertices[0] - Vertices[Vertices.Count - 1]);
            }
            return edges;
        }

        public Polygon TransformVertices(Matrix transform)
        {
            for (int i = 0; i < Vertices.Count; i++)
            {
                Vertices[i] = Vector2.Transform(Vertices[i], transform);
            }

            return this;
        }

        public void RotateDegrees(float angle)
        {
            if (angle < 0)
            {
                angle = 360 + angle % 360;
            }

            if (angle > 360)
            {
                angle = angle % 360;
            }

            if (angle % 90 == 0)
            {
                //right angle rotation
                if (angle == 90)
                {
                    for (int i = 0; i < Vertices.Count; i++)
                    {
                        Vertices[i] = new Vector2(-Vertices[i].Y, Vertices[i].X);
                    }
                }

                if (angle == 180)
                {
                    for (int i = 0; i < Vertices.Count; i++)
                    {
                        Vertices[i] = new Vector2(-Vertices[i].X, -Vertices[i].Y);
                    }
                }

                if (angle == 270)
                {
                    for (int i = 0; i < Vertices.Count; i++)
                    {
                        Vertices[i] = new Vector2(Vertices[i].Y, -Vertices[i].X);
                    }
                }
            }
            else
            {
                RotateRadians(angle * MathHelper.TwoPi / 360f);
            }
        }
        public void RotateDegrees(float angle, Vector2 center)
        {
            if (angle < 0)
            {
                angle = 360 + angle % 360;
            }

            if (angle > 360)
            {
                angle = angle % 360;
            }

            if (angle % 90 == 0)
            {
                //right angle rotation
                if (angle == 90)
                {
                    for (int i = 0; i < Vertices.Count; i++)
                    {
                        Vertices[i] = center + new Vector2(-Vertices[i].Y + center.Y, Vertices[i].X - center.X);
                    }
                }

                if (angle == 180)
                {
                    for (int i = 0; i < Vertices.Count; i++)
                    {
                        Vertices[i] = center + new Vector2(-Vertices[i].X + center.X, -Vertices[i].Y + center.Y);
                    }
                }

                if (angle == 270)
                {
                    for (int i = 0; i < Vertices.Count; i++)
                    {
                        Vertices[i] = center + new Vector2(Vertices[i].Y - center.Y, -Vertices[i].X + center.X);
                    }
                }
            }
            else
            {
                RotateRadians(angle * MathHelper.TwoPi / 360f, center);
            }
        }
        public void RotateRadians(float angle)
        {
            for (int i = 0; i < Vertices.Count; i++)
            {
                Vertices[i] = new Vector2((float)(Math.Cos(angle) * Vertices[i].X - Math.Sin(angle) * Vertices[i].Y),
                                            (float)(Math.Cos(angle) * Vertices[i].Y + Math.Sin(angle) * Vertices[i].X));
            }
        }
        public void RotateRadians(float angle, Vector2 center)
        {
            for (int i = 0; i < Vertices.Count; i++)
            {
                Vertices[i] -= center;
                Vertices[i] = center + new Vector2((float)(Math.Cos(angle) * Vertices[i].X - Math.Sin(angle) * Vertices[i].Y),
                                            (float)(Math.Cos(angle) * Vertices[i].Y + Math.Sin(angle) * Vertices[i].X));
            }
        }
        public void Flip(bool horizontally, bool vertically)
        {
            Flip(horizontally, vertically, this.GetCenter() - pos);
        }

        public void Flip(bool horizontally, bool vertically, Vector2 center)
        {
            if (horizontally)
            {
                for (int i = 0; i < Vertices.Count; i++)
                {
                    Vertices[i] = new Vector2(2 * center.X - Vertices[i].X, Vertices[i].Y);
                }
            }

            if (vertically)
            {
                for (int i = 0; i < Vertices.Count; i++)
                {
                    Vertices[i] = new Vector2(Vertices[i].X, 2 * center.Y - Vertices[i].Y);
                }
            }
        }
        public void Scale(Vector2 scale)
        {
            Scale(scale, Vector2.Zero);
        }
        public void Scale(Vector2 scale, Vector2 center)
        {
            if (Math.Sign(scale.X * scale.Y) == -1)
            {
                //invert vertice order
                List<Vector2> oldVertices = Vertices.ToList();
                int j = Vertices.Count - 1;
                for (int i = 0; i < Vertices.Count; i++, j--)
                {
                    Vertices[i] = center + (oldVertices[j] - center) * scale;
                }
            }
            else
            {
                //normal order
                for (int i = 0; i < Vertices.Count; i++)
                {
                    Vertices[i] = center + (Vertices[i] - center) * scale;
                }
            }

        }

        public Type GetCollisionType() => typeof(Polygon);

        public virtual object Clone()
        {
            Polygon clone = (Polygon)MemberwiseClone();
            clone.Vertices = Vertices.ToList();
            return clone;
        }


        public static Polygon GetRandomConvex(Vector2 pos, Random rand, float radius, float angleMin, float angleRange)
        {
            Polygon poly = new Polygon(pos, new List<Vector2>());

            float startAngle = (float)rand.NextDouble() * MathHelper.TwoPi;

            for (float a = 0f; a < MathHelper.TwoPi;)         // full circle
            {
                poly.Vertices.Add(new Vector2((float)Math.Cos(startAngle + a) * radius, (float)Math.Sin(startAngle + a) * radius));
                a += (float)((angleMin + (angleRange * rand.NextDouble())) * Math.PI / 180f);
            }

            return poly;
        }
        public static Polygon GetRandomConvex(Vector2 pos, Random rand, float radius)
        {
            int maxAngle = rand.Next(45, 360 / 3);
            int minAngle = rand.Next(maxAngle);
            return GetRandomConvex(pos, rand, radius, minAngle, maxAngle - minAngle);
        }

        public static Polygon GetCircle(Vector2 pos, float radius, int vertices)
        {
            Polygon poly = new Polygon(pos, new List<Vector2>());
            for (int i = 0; i < vertices; i++)
            {
                float a = i * MathHelper.TwoPi / vertices;
                poly.Vertices.Add(new Vector2((float)Math.Cos(a) * radius, (float)Math.Sin(a) * radius));
            }
            return poly;
        }
        public static Polygon GetCone(Vector2 pos, float radius, float angleStart, float angle, int vertices)
        {
            Polygon poly = new Polygon(pos, new List<Vector2>() { Vector2.Zero });
            float a = angleStart;
            float plus = angle / (vertices - 1);
            for (int i = 0; i < vertices; i++)
            {
                poly.Vertices.Add(new Vector2((float)Math.Cos(a) * radius, (float)Math.Sin(a) * radius));
                a += plus;
            }
            return poly;
        }
        /*public static Polygon GetCircle(Vector2 pos, float radius, int vertices)
        {
            Polygon poly = new Polygon(pos, new List<Vector2>());
            
            float anglePlus = (float)(MathHelper.TwoPi / (float)vertices);

            for (float a = 0f; a < MathHelper.TwoPi; a += anglePlus)         // full circle
            {
                poly.vertices.Add(new Vector2((float)Math.Cos(a) * radius, (float)Math.Sin(a) * radius));
            }

            return poly;
        }*/

        public static Polygon GetRectangle(Vector2 pos, Vector2 size, Vector2 originNormalized)
        {
            Vector2 originInverse = Vector2.One - originNormalized;
            Vector2 min = -size * originNormalized;
            Vector2 max = size * originInverse;
            return new Polygon(pos, new List<Vector2>()
            {
                min,
                new Vector2(max.X, min.Y),
                max,
                new Vector2(min.X, max.Y)
            });
        }

        public void Draw(SpriteBatch spriteBatch, Color color, float depth)
        {
            spriteBatch.DrawPolygon(this, color, depth);
        }

        public PrimitiveLineRing Outline() => new PrimitiveLineRing(Vertices.Select(f => f + pos).ToList());

        public static IEnumerable<(float, float)> GetDistanceSquaredToIndex(List<Vector2> vertices, Vector2 target)
        {
            if (vertices.Count == 0)
            {
                yield break;
            }

            // check edges
            for (int i = 0; i < vertices.Count - 1; i++)
            {
                Vector2 lineStart = vertices[i];
                Vector2 lineDir = vertices[i + 1] - lineStart;
                if (lineDir == Vector2.Zero)
                {
                    continue;
                }
                //float lineDirLength = lineDir.Length();
                //Vector2 lineDirN = lineDir / lineDirLength;

                //float dirLengthSq = lineDir.LengthSquared();
                Vector2 toPos = target - lineStart;
                float dotTimesLineLength = Vector2.Dot(lineDir, toPos);
                if (dotTimesLineLength < 0
                    || dotTimesLineLength > lineDir.LengthSquared())
                {
                    continue;
                }

                float lineLength = lineDir.Length();
                float dot = dotTimesLineLength / lineLength;
                float distSq = Vector2.Dot((lineDir / lineLength).GetRotate90(), toPos);
                distSq *= distSq;
                yield return (distSq, i + dot / lineLength);
            }

            // check vertices
            for (int i = 0; i < vertices.Count; i++)
            {
                float distSq = (vertices[i] - target /* to prioritize docking inside of lines less */
                    ).LengthSquared();
                yield return (distSq, i);
            }
        }

        public static float? GetNearestIndex(List<Vector2> vertices, Vector2 target)
        {
            return GetDistanceSquaredToIndex(vertices, target).MinBy(f => f.Item1).Item2;
        }

        public CollisionResultPolygonExtended DistToCircleExtended(Circle circle, Vector2 dir)
        {
            return Collision.DistPolygonCircleExtended(this, circle, dir);
        }

        public Vector2 GetPos(int index)
        {
            return Pos + Vertices[index];
        }

        public void SkewX(float angle)
        {
            SkewX(Vertices, angle);
        }
        public void SkewY(float angle)
        {
            SkewY(Vertices, angle);
        }

        public static void SkewX(IList<Vector2> vertices, float angle)
        {
            if (vertices.Count == 0)
            {
                return;
            }
            Vector2 center;
            float shift;
            SkewInit(vertices, angle, out center, out shift);
            for (int i = 0; i < vertices.Count; i++)
            {
                Vector2 offset = vertices[i] - center;
                vertices[i] = center + new Vector2(offset.X - offset.Y * shift, offset.Y);
            }
        }
        public static void SkewY(IList<Vector2> vertices, float angle)
        {
            if (vertices.Count == 0)
            {
                return;
            }
            Vector2 center;
            float shift;
            SkewInit(vertices, angle, out center, out shift);
            for (int i = 0; i < vertices.Count; i++)
            {
                Vector2 offset = vertices[i] - center;
                vertices[i] = center + new Vector2(offset.X, offset.Y - offset.X * shift);
            }
        }

        private static void SkewInit(IList<Vector2> vertices, float angle, out Vector2 center, out float shift)
        {
            Vector2 min = vertices[0], max = vertices[0];
            for (int i = 0; i < vertices.Count; i++)
            {
                if (vertices[i].X > max.X)
                {
                    max.X = vertices[i].X;
                }
                else if (vertices[i].X < min.X)
                {
                    min.X = vertices[i].X;
                }

                if (vertices[i].Y > max.Y)
                {
                    max.Y = vertices[i].Y;
                }
                else if (vertices[i].Y < min.Y)
                {
                    min.Y = vertices[i].Y;
                }
            }
            center = (min + max) / 2f;
            shift = MathF.Sin(angle);
        }
    }
}
