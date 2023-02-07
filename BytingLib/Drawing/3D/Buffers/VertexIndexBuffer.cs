namespace BytingLib
{
    public class VertexIndexBuffer
    {
        public VertexBuffer VertexBuffer { get; }
        public IndexBuffer IndexBuffer { get; }
        public PrimitiveType PrimitiveType { get; }

        public VertexIndexBuffer(VertexBuffer vertexBuffer, IndexBuffer indexBuffer, PrimitiveType primitiveType)
        {
            VertexBuffer = vertexBuffer;
            IndexBuffer = indexBuffer;
            PrimitiveType = primitiveType;
        }

        public static VertexIndexBuffer Create<V>(GraphicsDevice gDevice, V[] vertices, short[] indices, PrimitiveType primitiveType) where V : struct, IVertexType
        {
            var vertexBuffer = new VertexBuffer(gDevice, vertices.GetType().GetElementType(), vertices.Length, BufferUsage.WriteOnly);
            vertexBuffer.SetData(vertices);

            var indexBuffer = new IndexBuffer(gDevice, IndexElementSize.SixteenBits, indices.Length, BufferUsage.WriteOnly);
            indexBuffer.SetData(indices);

            return new VertexIndexBuffer(vertexBuffer, indexBuffer, primitiveType);
        }


        static VertexIndexBuffer? triangle;
        public static VertexIndexBuffer GetTriangle(GraphicsDevice gDevice)
        {
            if (triangle == null)
            {
                triangle = Create(gDevice, new VertexPositionNormal[]
                    {  new(Vector3.Zero, Vector3.UnitZ), new(Vector3.UnitY, Vector3.UnitZ), new(Vector3.UnitX, Vector3.UnitZ) },
                    new short[] { 0, 1, 2 },
                    PrimitiveType.TriangleList);
            }
            return triangle;
        }

        static VertexIndexBuffer? line;
        public static VertexIndexBuffer GetLine(GraphicsDevice gDevice)
        {
            if (line == null)
            {
                line = Create(gDevice, new VertexPosition[]
                {  new(Vector3.Zero), new(Vector3.UnitX) },
                new short[] { 0, 1 },
                PrimitiveType.LineList);
            }
            return line;
        }

        static VertexIndexBuffer? box;
        public static VertexIndexBuffer GetBox(GraphicsDevice gDevice)
        {
            if (box == null)
            {
                // draw 6 quads, where each quad has 6 indices and 4 vertices
                const int faces = 6;
                VertexPositionNormal[] vertices = new VertexPositionNormal[faces * 4];
                short[] indices = new short[faces * 6];

                // indices
                // 3 - 2
                // | / |
                // 0 - 1
                short verticesIndex = 0;
                int indicesIndex = 0;
                short startIndex = verticesIndex;
                for (int face = 0; face < faces; face++)
                {
                    indices[indicesIndex++] = (short)(startIndex + 0);
                    indices[indicesIndex++] = (short)(startIndex + 2);
                    indices[indicesIndex++] = (short)(startIndex + 1);
                    indices[indicesIndex++] = (short)(startIndex + 0);
                    indices[indicesIndex++] = (short)(startIndex + 3);
                    indices[indicesIndex++] = (short)(startIndex + 2);
                    startIndex += 4;
                }

                Vector3 Min = new Vector3(-0.5f);
                Vector3 Max = new Vector3(0.5f);

                // +z face
                vertices[verticesIndex++] = new(new Vector3(Min.X, Min.Y, Max.Z), Vector3.UnitZ);
                vertices[verticesIndex++] = new(new Vector3(Max.X, Min.Y, Max.Z), Vector3.UnitZ);
                vertices[verticesIndex++] = new(new Vector3(Max.X, Max.Y, Max.Z), Vector3.UnitZ);
                vertices[verticesIndex++] = new(new Vector3(Min.X, Max.Y, Max.Z), Vector3.UnitZ);

                // +y face
                vertices[verticesIndex++] = new(new Vector3(Min.X, Max.Y, Max.Z), Vector3.UnitY);
                vertices[verticesIndex++] = new(new Vector3(Max.X, Max.Y, Max.Z), Vector3.UnitY);
                vertices[verticesIndex++] = new(new Vector3(Max.X, Max.Y, Min.Z), Vector3.UnitY);
                vertices[verticesIndex++] = new(new Vector3(Min.X, Max.Y, Min.Z), Vector3.UnitY);

                // -z face
                vertices[verticesIndex++] = new(new Vector3(Min.X, Max.Y, Min.Z), -Vector3.UnitZ);
                vertices[verticesIndex++] = new(new Vector3(Max.X, Max.Y, Min.Z), -Vector3.UnitZ);
                vertices[verticesIndex++] = new(new Vector3(Max.X, Min.Y, Min.Z), -Vector3.UnitZ);
                vertices[verticesIndex++] = new(new Vector3(Min.X, Min.Y, Min.Z), -Vector3.UnitZ);

                // -y face
                vertices[verticesIndex++] = new(new Vector3(Min.X, Min.Y, Min.Z), -Vector3.UnitY);
                vertices[verticesIndex++] = new(new Vector3(Max.X, Min.Y, Min.Z), -Vector3.UnitY);
                vertices[verticesIndex++] = new(new Vector3(Max.X, Min.Y, Max.Z), -Vector3.UnitY);
                vertices[verticesIndex++] = new(new Vector3(Min.X, Min.Y, Max.Z), -Vector3.UnitY);

                // +x face
                vertices[verticesIndex++] = new(new Vector3(Max.X, Min.Y, Max.Z), Vector3.UnitX);
                vertices[verticesIndex++] = new(new Vector3(Max.X, Min.Y, Min.Z), Vector3.UnitX);
                vertices[verticesIndex++] = new(new Vector3(Max.X, Max.Y, Min.Z), Vector3.UnitX);
                vertices[verticesIndex++] = new(new Vector3(Max.X, Max.Y, Max.Z), Vector3.UnitX);

                // -x face
                vertices[verticesIndex++] = new(new Vector3(Min.X, Min.Y, Min.Z), -Vector3.UnitX);
                vertices[verticesIndex++] = new(new Vector3(Min.X, Min.Y, Max.Z), -Vector3.UnitX);
                vertices[verticesIndex++] = new(new Vector3(Min.X, Max.Y, Max.Z), -Vector3.UnitX);
                vertices[verticesIndex++] = new(new Vector3(Min.X, Max.Y, Min.Z), -Vector3.UnitX);

                box = Create(gDevice, vertices, indices, PrimitiveType.TriangleList);
            }
            return box;
        }

        static VertexIndexBuffer? sphere;
        public static VertexIndexBuffer GetSphere(GraphicsDevice gDevice)
        {
            if (sphere == null)
            {
                var v = Icosahedron.VerticesSub;
                var ind = Icosahedron.IndicesSub;

                VertexPositionNormal[] vertices = new VertexPositionNormal[v.Length];
                short[] indices = new short[ind.Length * 3];
                int indicesIndex = 0;
                short verticesIndex = 0;

                for (int i = 0; i < ind.Length; i++)
                {
                    indices[indicesIndex++] = (short)(verticesIndex + ind[i][0]);
                    indices[indicesIndex++] = (short)(verticesIndex + ind[i][1]);
                    indices[indicesIndex++] = (short)(verticesIndex + ind[i][2]);
                }
                for (int i = 0; i < v.Length; i++)
                    vertices[verticesIndex++] = new VertexPositionNormal(v[i], v[i]);

                sphere = Create(gDevice, vertices, indices, PrimitiveType.TriangleList);
            }
            return sphere;
        }

        static VertexIndexBuffer? openCylinder;
        public static VertexIndexBuffer GetOpenCylinder(GraphicsDevice gDevice)
        {
            if (openCylinder == null)
            {
                int indicesIndex = 0;
                short verticesIndex = 0;

                //draw cylinder without caps
                // the cylinder has <FACES> faces -> <FACES>*2 vertices <FACES>*6 indices
                const int FACES = 12;
                VertexPositionNormal[] vertices = new VertexPositionNormal[FACES * 2];
                short[] indices = new short[FACES * 6];

                // indices
                // ...
                // 4 - 5
                // | / |
                // 2 - 3
                // | / |
                // 0 - 1
                short startIndex = verticesIndex;
                for (int i = 0; i < FACES; i++)
                {
                    indices[indicesIndex++] = (short)(startIndex + 0);
                    indices[indicesIndex++] = (short)(startIndex + 3);
                    indices[indicesIndex++] = (short)(startIndex + 1);
                    indices[indicesIndex++] = (short)(startIndex + 0);
                    indices[indicesIndex++] = (short)(startIndex + 2);
                    indices[indicesIndex++] = (short)(startIndex + 3);
                    startIndex += 2;
                }
                // last two vertices are the first two. So let the last indices point to them
                indices[indicesIndex - 5] = (short)(verticesIndex + 1);
                indices[indicesIndex - 2] = (short)(verticesIndex + 0);
                indices[indicesIndex - 1] = (short)(verticesIndex + 1);

                Vector3 xAxis = Vector3.UnitY;
                Vector3 yAxis = Vector3.UnitZ;
                float toAngle = -MathHelper.TwoPi / FACES;
                Vector3 p1 = Vector3.UnitX;
                Vector3 p2 = Vector3.Zero;
                for (int i = 0; i < FACES; i++)
                {
                    float angle = i * toAngle;
                    Vector3 n = MathF.Cos(angle) * xAxis + MathF.Sin(angle) * yAxis;
                    vertices[verticesIndex++] = new(p1 + n, n);
                    vertices[verticesIndex++] = new(p2 + n, n);
                }

                openCylinder = Create(gDevice, vertices, indices, PrimitiveType.TriangleList);
            }
            return openCylinder;
        }

        static VertexIndexBuffer? cylinder;
        public static VertexIndexBuffer GetCylinder(GraphicsDevice gDevice)
        {
            if (cylinder == null)
            {
                int indicesIndex = 0;
                short verticesIndex = 0;

                // the cylinder has <FACES> faces on the side -> <FACES>*2 vertices <FACES>*6 indices
                // and two disk faces on the caps -> <FACES>*2 vertices and <FACES>*2 indices
                const int FACES = 12;
                const int WallVertices = FACES * 2;
                const int CapVerticesTotal = FACES * 2;
                const int IndicesPerCap = 3 * (FACES - 1);
                VertexPositionNormal[] vertices = new VertexPositionNormal[WallVertices + CapVerticesTotal];
                short[] indices = new short[FACES * 6 + IndicesPerCap * 2];

                // indices
                // ...
                // 4 - 5
                // | / |
                // 2 - 3
                // | / |
                // 0 - 1
                short currentIndex = verticesIndex;
                for (int i = 0; i < FACES; i++)
                {
                    indices[indicesIndex++] = (short)(currentIndex + 0);
                    indices[indicesIndex++] = (short)(currentIndex + 3);
                    indices[indicesIndex++] = (short)(currentIndex + 1);
                    indices[indicesIndex++] = (short)(currentIndex + 0);
                    indices[indicesIndex++] = (short)(currentIndex + 2);
                    indices[indicesIndex++] = (short)(currentIndex + 3);
                    currentIndex += 2;
                }
                // last two vertices are the first two. So let the last indices point to them
                indices[indicesIndex - 5] = (short)(verticesIndex + 1);
                indices[indicesIndex - 2] = (short)(verticesIndex + 0);
                indices[indicesIndex - 1] = (short)(verticesIndex + 1);

                // indices for caps
                // 1 - 2 - 3 - 4 - 5
                // | /   /   /   /
                // 0 - - - - - - 
                for (int cap = 0; cap < 2; cap++)
                {
                    short centerIndex = currentIndex;
                    for (int i = 1; i < FACES; i++)
                    {
                        indices[indicesIndex++] = centerIndex;
                        indices[indicesIndex++] = currentIndex;
                        indices[indicesIndex++] = ++currentIndex;
                    }
                    currentIndex++;
                }


                // cylinder walls and caps
                Vector3 xAxis = Vector3.UnitY;
                Vector3 yAxis = Vector3.UnitZ;
                float toAngle = -MathHelper.TwoPi / FACES;
                Vector3 capNormal1 = Vector3.UnitX;
                Vector3 capNormal2 = -capNormal1;

                int cap1VerticesIndex = verticesIndex + WallVertices;
                int cap2VerticesIndex = cap1VerticesIndex + CapVerticesTotal;

                for (int i = 0; i < FACES; i++)
                {
                    float angle = i * toAngle;
                    Vector3 n = MathF.Cos(angle) * xAxis + MathF.Sin(angle) * yAxis;
                    Vector3 p1 = n;
                    Vector3 p2 = Vector3.UnitX + n;
                    // cylinder walls
                    vertices[verticesIndex++] = new(p1, n);
                    vertices[verticesIndex++] = new(p2, n);

                    // cylinder caps
                    vertices[cap1VerticesIndex++] = new(p2, capNormal1);
                    vertices[--cap2VerticesIndex] = new(p1, capNormal2);
                }

                cylinder = Create(gDevice, vertices, indices, PrimitiveType.TriangleList);
            }
            return cylinder;
        }
    }
}
