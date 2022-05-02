using Microsoft.Xna.Framework;

namespace BytingLib
{
    public class Polygon : IShape
    {
        private Vector2 pos;

        public List<Vector2> vertices;

        public bool closed, startCorner, endCorner; //start and end corner when polygon is open

        public Polygon(Vector2 pos, List<Vector2> vertices, bool closed = true)
        {
            this.pos = pos;
            this.vertices = vertices;
            this.closed = closed;
            startCorner = endCorner = true;
        }

        public Vector2 Pos { get => pos; set => pos = value; }
        public float X { get => pos.X; set => pos.X = value; }
        public float Y { get => pos.Y; set => pos.Y = value; }

        public bool CollidesWith(IShape shape) => Collision.GetCollision(this, shape);
        public CollisionResult DistanceTo(IShape shape, Vector2 dir) => Collision.GetDistance(this, shape, dir);

        public Polygon ApplyPosition()
        {
            for (int i = 0; i < vertices.Count; i++)
            {
                vertices[i] += pos;
            }
            pos = Vector2.Zero;
            return this;
        }

        public Rect GetBoundingRectangle()
        {
            Vector2 minPos, maxPos;
            if (vertices.Count > 0)
            {
                minPos = maxPos = vertices[0];
                for (int i = 1; i < vertices.Count; i++)
                {
                    minPos = Vector2.Min(minPos, vertices[i]);
                    maxPos = Vector2.Max(maxPos, vertices[i]);
                }

                return new Rect(pos + minPos, maxPos - minPos);
            }
            else
                return new Rect(pos, Vector2.Zero);
        }

        public List<Vector2> GetEdges()
        {
            List<Vector2> edges = new List<Vector2>();
            if (vertices.Count > 1)
            {
                for (int i = 0; i < vertices.Count - 1; i++)
                    edges.Add(vertices[i + 1] - vertices[i]);

                //if (vertices.Count > 2)
                if (closed)
                    edges.Add(vertices[0] - vertices[vertices.Count - 1]);
            }
            return edges;
        }

        public List<Vector2> GetClosedEdges()
        {
            List<Vector2> edges = new List<Vector2>();
            if (vertices.Count > 1)
            {
                for (int i = 0; i < vertices.Count - 1; i++)
                    edges.Add(vertices[i + 1] - vertices[i]);
                edges.Add(vertices[0] - vertices[vertices.Count - 1]);
            }
            return edges;
        }

        public Polygon Transform(Matrix transform)
        {
            for (int i = 0; i < vertices.Count; i++)
                vertices[i] = Vector2.Transform(vertices[i], transform);

            return this;
        }

        public void RotateDegrees(float angle)
        {
            if (angle < 0)
                angle = 360 + angle % 360;
            if (angle > 360)
                angle = angle % 360;

            if (angle % 90 == 0)
            {
                //right angle rotation
                if (angle == 90)
                    for (int i = 0; i < vertices.Count; i++)
                        vertices[i] = new Vector2(-vertices[i].Y, vertices[i].X);
                if (angle == 180)
                    for (int i = 0; i < vertices.Count; i++)
                        vertices[i] = new Vector2(-vertices[i].X, -vertices[i].Y);
                if (angle == 270)
                    for (int i = 0; i < vertices.Count; i++)
                        vertices[i] = new Vector2(vertices[i].Y, -vertices[i].X);
            }
            else
                RotateRadians(angle * MathHelper.TwoPi / 360f);
        }
        public void RotateDegrees(float angle, Vector2 center)
        {
            if (angle < 0)
                angle = 360 + angle % 360;
            if (angle > 360)
                angle = angle % 360;

            if (angle % 90 == 0)
            {
                //right angle rotation
                if (angle == 90)
                    for (int i = 0; i < vertices.Count; i++)
                        vertices[i] = center + new Vector2(-vertices[i].Y + center.Y, vertices[i].X - center.X);
                if (angle == 180)
                    for (int i = 0; i < vertices.Count; i++)
                        vertices[i] = center + new Vector2(-vertices[i].X + center.X, -vertices[i].Y + center.Y);
                if (angle == 270)
                    for (int i = 0; i < vertices.Count; i++)
                        vertices[i] = center + new Vector2(vertices[i].Y - center.Y, -vertices[i].X + center.X);
            }
            else
                RotateRadians(angle * MathHelper.TwoPi / 360f, center);
        }
        public void RotateRadians(float angle)
        {
            for (int i = 0; i < vertices.Count; i++)
            {
                vertices[i] = new Vector2((float)(Math.Cos(angle) * vertices[i].X - Math.Sin(angle) * vertices[i].Y),
                                            (float)(Math.Cos(angle) * vertices[i].Y + Math.Sin(angle) * vertices[i].X));
            }
        }
        public void RotateRadians(float angle, Vector2 center)
        {
            for (int i = 0; i < vertices.Count; i++)
            {
                vertices[i] -= center;
                vertices[i] = center + new Vector2((float)(Math.Cos(angle) * vertices[i].X - Math.Sin(angle) * vertices[i].Y),
                                            (float)(Math.Cos(angle) * vertices[i].Y + Math.Sin(angle) * vertices[i].X));
            }
        }
        public void Flip(bool horizontally, bool vertically)
        {
            Flip(horizontally, vertically, this.GetCenter() - pos);
        }

        public void Flip(bool horizontally, bool vertically, Vector2 center)
        {
            if (horizontally)
                for (int i = 0; i < vertices.Count; i++)
                    vertices[i] = new Vector2(2 * center.X - vertices[i].X, vertices[i].Y);
            if (vertically)
                for (int i = 0; i < vertices.Count; i++)
                    vertices[i] = new Vector2(vertices[i].X, 2 * center.Y - vertices[i].Y);
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
                List<Vector2> oldVertices = vertices.ToList();
                int j = vertices.Count - 1;
                for (int i = 0; i < vertices.Count; i++, j--)
                    vertices[i] = center + (oldVertices[j] - center) * scale;
            }
            else
            {
                //normal order
                for (int i = 0; i < vertices.Count; i++)
                    vertices[i] = center + (vertices[i] - center) * scale;
            }

        }


        public object Clone()
        {
            Polygon clone = (Polygon)MemberwiseClone();
            clone.vertices = vertices.ToList();
            return clone;
        }


        public static Polygon GetRandomConvex(Vector2 pos, Random rand, float radius, float angleMin, float angleRange)
        {
            Polygon poly = new Polygon(pos, new List<Vector2>());

            float startAngle = (float)rand.NextDouble() * MathHelper.TwoPi;

            for (float a = 0f; a < MathHelper.TwoPi;)         // full circle
            {
                poly.vertices.Add(new Vector2((float)Math.Cos(startAngle + a) * radius, (float)Math.Sin(startAngle + a) * radius));
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

        public static Polygon GetCircleOpen(Vector2 pos, float radius, int vertices)
        {
            Polygon poly = new Polygon(pos, new List<Vector2>());
            for (int i = 0; i < vertices; i++)
            {
                float a = i * MathHelper.TwoPi / vertices;
                poly.vertices.Add(new Vector2((float)Math.Cos(a) * radius, (float)Math.Sin(a) * radius));
            }
            return poly;
        }
        public static Polygon GetCircleClosed(Vector2 pos, float radius, int vertices)
        {
            Polygon poly = GetCircleOpen(pos, radius, vertices - 1);
            poly.vertices.Add(poly.vertices[0]);
            return poly;
        }
        public static Polygon GetCirclePart(Vector2 pos, float radius, float angle, float fov, int vertices)
        {
            Polygon poly = new Polygon(pos, new List<Vector2>() { Vector2.Zero });
            float a = angle - fov / 2f;
            float plus = fov / (vertices - 1);
            for (int i = 0; i < vertices; i++)
            {
                poly.vertices.Add(new Vector2((float)Math.Cos(a) * radius, (float)Math.Sin(a) * radius));
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
    }
}
