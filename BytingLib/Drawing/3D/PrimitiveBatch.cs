namespace BytingLib
{
    public class PrimitiveBatch : IBufferBatch, IDisposable
    {
        private DynamicVertexBuffer? InstanceBuffer;
        private readonly GraphicsDevice gDevice;
        private readonly string positionNormalTechnique;
        private readonly string positionTechnique;
        private readonly int growBuffersBy;

        public InstancesLine Lines { get; } = new();
        public InstancesTriangle Triangles { get; } = new();

        List<(IInstances<VertexInstanceTransformColor> Instances, VertexIndexBuffer Buffer)> lineInstances = new();
        List<(IInstances<VertexInstanceTransformColor> Instances, VertexIndexBuffer Buffer)> triInstances = new();

        public PrimitiveBatch(GraphicsDevice gDevice, string positionNormalTechnique, string positionTechnique, int growBuffersBy = 64)
        {
            this.gDevice = gDevice;
            this.positionNormalTechnique = positionNormalTechnique;
            this.positionTechnique = positionTechnique;
            this.growBuffersBy = growBuffersBy;

            lineInstances.Add((Lines, VertexIndexBuffer.GetLine(gDevice)));
            triInstances.Add((Triangles, VertexIndexBuffer.GetTriangle(gDevice)));
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

                using (shader.UseTechnique(positionNormalTechnique))
                {
                    foreach (var instancesAndBuffer in triInstances)
                    {
                        InstanceDrawer<VertexInstanceTransformColor>.DrawBuffers(shader, instancesAndBuffer.Instances,
                            GetInstanceBuffer(instancesAndBuffer.Instances.Count), instancesAndBuffer.Buffer);
                    }
                }
                using (shader.UseTechnique(positionTechnique))
                {
                    foreach (var instancesAndBuffer in lineInstances)
                    {
                        InstanceDrawer<VertexInstanceTransformColor>.DrawBuffers(shader, instancesAndBuffer.Instances,
                            GetInstanceBuffer(instancesAndBuffer.Instances.Count), instancesAndBuffer.Buffer);
                    }
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
    }
}
