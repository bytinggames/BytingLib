namespace BytingLib
{
    class VertexPartGL
    {
        public VertexPartGL(VertexElement vertexElement, byte[] bufferBytes, int vertexElementSize)
        {
            VertexElement = vertexElement;
            BufferBytes = bufferBytes;
            VertexElementSize = vertexElementSize;
        }

        public VertexElement VertexElement { get; }
        public byte[] BufferBytes { get; }
        public int VertexElementSize { get; }
    }
}
