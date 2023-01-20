using Microsoft.Xna.Framework.Graphics;

namespace BytingLib
{
    public class PrimitiveBatch : IDisposable
    {
        private List<IMultiBuffer> buffers = new();
        private DynamicVertexBuffer? InstanceBuffer;
        private readonly GraphicsDevice gDevice;
        private readonly int growBuffersBy;

        public BufferWrapper<Line3> Lines { get; }
        public BufferWrapper<Triangle3> Triangles { get; }

        DynamicVertexBuffer GetInstanceBuffer(int capacity)
        {
            if (InstanceBuffer != null && InstanceBuffer.VertexCount >= capacity)
                return InstanceBuffer;
            capacity = capacity * 3 / 2; // make 1.5 times as big
            capacity = (int)MathF.Ceiling((float)capacity / growBuffersBy) * growBuffersBy; // grow in steps
            InstanceBuffer?.Dispose();
            InstanceBuffer = new DynamicVertexBuffer(gDevice, VertexInstanceTransformColor.vertexDeclaration, capacity, BufferUsage.WriteOnly);
            return InstanceBuffer;
        }

        public PrimitiveBatch(GraphicsDevice GraphicsDevice, int growBuffersBy = 64)
        {
            gDevice = GraphicsDevice;
            this.growBuffersBy = growBuffersBy;
            Lines = new(() => AddBuffer(new LineBuffer(gDevice)), CanAdd);
            Triangles = new(() => AddBuffer(new TriangleBuffer(gDevice)), CanAdd);
        }

        private bool CanAdd() => begun;

        private T AddBuffer<T>(T buffer) where T : IMultiBuffer
        {
            buffers.Add(buffer);
            return buffer;
        }

        bool begun = false;

        public void Begin()
        {
            if (begun)
                throw new BytingException("Begin() has already been called. Call End() first.");
            begun = true;
        }

        public void End(Effect effect)
        {
            if (!begun)
                throw new BytingException("Begin() has not yet been called.");

            try
            {
                if (buffers.Count == 0)
                    return;

                var instanceBuffer = GetInstanceBuffer(buffers.Max(f => f.Count));

                for (int i = 0; i < buffers.Count; i++)
                {
                    buffers[i].Draw(instanceBuffer, effect);
                }
            }
            finally
            {
                begun = false;
            }
        }

        public void Dispose()
        {
            InstanceBuffer?.Dispose();
            for (int i = 0; i < buffers.Count; i++)
            {
                buffers[i].Dispose();
            }
        }
    }
}
