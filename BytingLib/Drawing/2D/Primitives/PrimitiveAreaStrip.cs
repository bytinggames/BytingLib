using System.Linq;

namespace BytingLib
{
    public class PrimitiveAreaStrip : PrimitiveArea
    {
        public Vector2[] Vertices { get; set; }

        private float[]? lengths;
        private float totalLength;
        public float TotalLength
        {
            get
            {
                if (lengths == null)
                    CalculateLengths();
                return totalLength;
            }
        }

        public PrimitiveAreaStrip(Vector2[] vertices)
        {
            this.Vertices = vertices;
        }

        public PrimitiveAreaStrip(Rect rect)
        {
            Vector2 p1 = rect.Pos;
            Vector2 p2 = rect.Pos + rect.Size;
            Vertices = new Vector2[4];
            Vertices[0] = new Vector2(p1.X, p2.Y);
            Vertices[1] = p1;
            Vertices[2] = p2;
            Vertices[3] = new Vector2(p2.X, p1.Y);
        }

        public PrimitiveAreaStrip(PrimitiveLineStrip line, float thickness, float anchor = 0f)
        {
            if (line.Vertices.Count <= 1)
            {
                Vertices = new Vector2[0];
                return;
            }

            Vertices = new Vector2[line.Vertices.Count * 2];

            float t = thickness / 2f;

            Vector2 a = line.Vertices[1] - line.Vertices[0];
            Vector2 b = default;

            for (int i = 0; i < line.Vertices.Count; i++)
            {
                //Vector2 a = line.Vertices[i] - line.Vertices[(i + line.Vertices.Length - 1) % line.Vertices.Length];
                if (i < line.Vertices.Count - 1)
                    b = line.Vertices[i] - line.Vertices[i + 1];
                float angleA = MathF.Atan2(a.Y, a.X);
                float angleB = MathF.Atan2(b.Y, b.X);
                float angleDistHalved = MathExtension.AngleDistance(angleA, angleB) / 2f;
                float sin = MathF.Sin(angleDistHalved);
                float x = t / sin;
                float angleToCorner = angleA + angleDistHalved;
                Vector2 dirToCorner = new Vector2(MathF.Cos(angleToCorner), MathF.Sin(angleToCorner)) * x;

                Vertices[i * 2] = line.Vertices[i] + dirToCorner * (1f - anchor);
                Vertices[i * 2 + 1] = line.Vertices[i] + dirToCorner * (-1f - anchor);

                a = -b;
            }
        }

        //public PrimitiveArea Outline(float thickness)
        //{

        //}

        public override PrimitiveLineRing Outline()
        {
            return new PrimitiveLineRing(this);
        }

        public override void Draw(GraphicsDevice gDevice)
        {
            //var v = new VertexBuffer(gDevice, new VertexDeclaration(new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0)), Vertices.Length, BufferUsage.WriteOnly);

            //v.SetData(Vertices);

            //Stopwatch sw = new Stopwatch();
            //sw.Start();
            //for (int i = 0; i < 1000; i++)
            //{
            //    gDevice.SetVertexBuffer(v);
            //    gDevice.DrawPrimitives(PrimitiveType.TriangleStrip, 0, Vertices.Length - 2);

            //}
            //sw.Stop();

            var arr = Vertices.Select(f => new VertexPosition(new Vector3(f, 0f))).ToArray();
            //sw.Restart();
            //for (int i = 0; i < 1000; i++)
            //{
            gDevice.DrawUserPrimitives<VertexPosition>(PrimitiveType.TriangleStrip, arr, 0, Vertices.Length - 2);
            //}
            //sw.Stop();
        }


        /// <param name="length">ranging from 0 to 1 to draw only a part of the strip</param>
        public void Draw(GraphicsDevice gDevice, float length)
        {
            if (length <= 0f)
                return;
            if (length >= 1f)
            {
                Draw(gDevice);
                return;
            }
            VertexPosition[] arr = GetVerticesToLength(length)
                .Select(f => new VertexPosition(new Vector3(f, 0f)))
                .ToArray();
            gDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, arr, 0, arr.Length - 2);
        }

        private IEnumerable<Vector2> GetVerticesToLength(float length)
        {
            if (lengths == null)
                CalculateLengths();

            int i = 0;
            length *= totalLength;
            while (length > 0)
            {
                length -= lengths![i++];
            }

            // draw vertices up to i * 2 + 2 newly dynamic vertex
            int vertexCount = i * 2;

            float lerp = 1f - (-length / lengths![i - 1]);
            List<Vector2> endVertices = new List<Vector2>()
            {
                ((1f - lerp) * Vertices[vertexCount - 2] + lerp * Vertices[vertexCount]),
                ((1f - lerp) * Vertices[vertexCount - 1] + lerp * Vertices[vertexCount + 1])
            };

            vertexCount += 2;

            var arr = Vertices
                .Take(vertexCount - 2)
                .Concat(endVertices);
            return arr;
        }

        public void SetLengthsDirty()
        {
            lengths = null;
            totalLength = -1f;
        }

        private void CalculateLengths()
        {
            if (Vertices.Length <= 2)
            {
                lengths = new float[0];
                return;
            }

            lengths = new float[Vertices.Length / 2 - 1];
            Vector2 center = (Vertices[0] + Vertices[1]) / 2f;
            for (int i = 0; i < lengths.Length; i++)
            {
                int j = (i + 1) * 2;
                Vector2 nextCenter = (Vertices[j] + Vertices[j + 1]) / 2f;
                lengths[i] = (nextCenter - center).Length();
                center = nextCenter;
            }

            totalLength = lengths.Sum();
        }

        public PrimitiveAreaStrip Transform(Matrix matrix)
        {
            for (int i = 0; i < Vertices.Length; i++)
            {
                Vertices[i] = Vector2.Transform(Vertices[i], matrix);
            }
            return this;
        }

        public override void Draw(SpriteBatch spriteBatch, Color color)
        {
            spriteBatch.DrawStrip(spriteBatch.GetPixel(), Vertices, color);
        }
        public override void Draw(SpriteBatch spriteBatch, Color color, float layerDepth)
        {
            spriteBatch.DrawStrip(spriteBatch.GetPixel(), Vertices, color, layerDepth);
        }

        /// <param name="length">ranging from 0 to 1 to draw only a part of the strip</param>
        public void Draw(SpriteBatch spriteBatch, Color color, float layerDepth, float length)
        {
            if (length <= 0f)
                return;
            if (length >= 1f)
            {
                Draw(spriteBatch, color, layerDepth);
                return;
            }
            Vector2[] arr = GetVerticesToLength(length).ToArray();
            spriteBatch.DrawStrip(spriteBatch.GetPixel(), arr, color, layerDepth);
        }

        public void DrawGradient(SpriteBatch spriteBatch, Color color1, Color color2)
            => DrawGradient(spriteBatch, color1, color2, spriteBatch.DefaultDepth);
        public void DrawGradient(SpriteBatch spriteBatch, Color color1, Color color2, float depth)
        {
            spriteBatch.DrawStrip(spriteBatch.GetPixel(),
                    GetDrawableVertexPositionsGradient(color1, color2, depth)
                );
        }

        public VertexPositionColorTexture[] GetDrawableVertexPositionsGradient(Color color1, Color color2, float depth)
        {
            bool toggle = true;
            return Vertices
                    .Select(f => new VertexPositionColorTexture(new Vector3(f.X, f.Y, depth), (toggle = !toggle) ? color1 : color2, Vector2.Zero))
                    .ToArray();
        }
    }
}
