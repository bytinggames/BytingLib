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
    }
}
