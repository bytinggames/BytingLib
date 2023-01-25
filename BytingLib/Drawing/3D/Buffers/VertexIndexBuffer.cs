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
    }
}
