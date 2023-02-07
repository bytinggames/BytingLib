namespace BytingLib
{
    class VertexPartGL
    {
        public VertexElement VertexElement { get; }
        public byte[] BufferBytes { get; }
        public int VertexElementSize { get; }

        public VertexPartGL(VertexElement vertexElement, byte[] bufferBytes, int vertexElementSize)
        {
            VertexElement = vertexElement;
            BufferBytes = bufferBytes;
            VertexElementSize = vertexElementSize;
        }
    }
}
