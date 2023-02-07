namespace BytingLib
{
    public class PrimitiveBatch : IBufferBatch, IDisposable
    {
        private DynamicVertexBuffer? InstanceBuffer;
        private readonly GraphicsDevice gDevice;
        private readonly int growBuffersBy;

        public InstancesLine Lines { get; }
        public InstancesTriangle Triangles { get; } = new();
        public InstancesBox Boxes { get; } = new();
        public InstancesSphere Spheres { get; } = new();

        List<InstancesAndBuffer> instancesAndBuffers = new();

        public PrimitiveBatch(GraphicsDevice gDevice, int growBuffersBy = 64, float infinity = 1000f)
        {
            Lines = new(infinity);

            this.gDevice = gDevice;
            this.growBuffersBy = growBuffersBy;

            instancesAndBuffers.Add(new(Lines, VertexIndexBuffer.GetLine(gDevice)));
            instancesAndBuffers.Add(new(Triangles, VertexIndexBuffer.GetTriangle(gDevice)));
            instancesAndBuffers.Add(new(Boxes, VertexIndexBuffer.GetBox(gDevice)));
            instancesAndBuffers.Add(new(Spheres, VertexIndexBuffer.GetSphere(gDevice)));
        }

        protected DynamicVertexBuffer GetInstanceBuffer(int capacity)
        {
            if (InstanceBuffer != null && InstanceBuffer.VertexCount >= capacity)
                return InstanceBuffer;
            capacity = capacity * 3 / 2; // make 1.5 times as big
            capacity = (int)MathF.Ceiling((float)capacity / growBuffersBy) * growBuffersBy; // grow in steps
            InstanceBuffer?.Dispose();
            InstanceBuffer = new DynamicVertexBuffer(gDevice, VertexInstanceTransformColor.vertexDeclaration, capacity, BufferUsage.WriteOnly);
            return InstanceBuffer;
        }

        public bool CanAdd() => begun;

        bool begun = false;

        public void Begin()
        {
            if (begun)
                throw new BytingException("Begin() has already been called. Call End() first.");
            begun = true;
        }

        public void End(IShader shader)
        {
            if (!begun)
                throw new BytingException("Begin() has not yet been called.");

            try
            {
                DrawCustom();

                foreach (var f in instancesAndBuffers)
                {
                    if (f.Instances.Count == 0)
                        continue;
                    InstanceDrawer<VertexInstanceTransformColor>.DrawBuffers(shader, f.Instances,
                        GetInstanceBuffer(f.Instances.Count), f.Buffer);
                }
            }
            finally
            {
                begun = false;
            }
        }

        protected virtual void DrawCustom()
        {
        }

        public void Dispose()
        {
            InstanceBuffer?.Dispose();
        }



        class InstancesAndBuffer
        {
            public IInstances<VertexInstanceTransformColor> Instances { get; }
            public VertexIndexBuffer Buffer { get; }

            public InstancesAndBuffer(IInstances<VertexInstanceTransformColor> instances, VertexIndexBuffer buffer)
            {
                Instances = instances;
                Buffer = buffer;
            }
        }
    }
}
