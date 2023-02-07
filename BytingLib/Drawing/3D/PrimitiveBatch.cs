namespace BytingLib
{
    public class PrimitiveBatch : IBufferBatch, IDisposable
    {
        private DynamicVertexBuffer? InstanceBuffer;
        private readonly GraphicsDevice gDevice;
        private readonly int growBuffersBy;

        public InstancesLine Lines { get; }
        public InstancesTriangle Triangles { get; }
        public InstancesBox Boxes { get; } = new();
        public InstancesSphere Spheres { get; } = new();
        public InstancesOpenCylinder OpenCylinders { get; }
        public InstancesCylinder Cylinders { get; } = new();

        List<InstancesAndBuffer> instancesAndBuffers = new();

        public PrimitiveBatch(GraphicsDevice gDevice, int growBuffersBy = 64, float infinity = 1000f)
        {
            Lines = new(infinity);
            Triangles = new(infinity);
            OpenCylinders = new(infinity);

            this.gDevice = gDevice;
            this.growBuffersBy = growBuffersBy;

            instancesAndBuffers.Add(new(Lines, VertexIndexBuffer.GetLine(gDevice)));
            instancesAndBuffers.Add(new(Triangles, VertexIndexBuffer.GetTriangle(gDevice)));
            instancesAndBuffers.Add(new(Boxes, VertexIndexBuffer.GetBox(gDevice)));
            instancesAndBuffers.Add(new(Spheres, VertexIndexBuffer.GetSphere(gDevice)));
            instancesAndBuffers.Add(new(OpenCylinders, VertexIndexBuffer.GetOpenCylinder(gDevice)));
            instancesAndBuffers.Add(new(Cylinders, VertexIndexBuffer.GetCylinder(gDevice)));
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

        public void Draw(Capsule3 capsule, Color color)
        {
            Spheres.Draw(capsule.Sphere0, color);
            Spheres.Draw(capsule.Sphere1, color);
            OpenCylinders.Draw(capsule.AxisRadius, color, capsule.SphereDistance);
        }

        public void Draw(Shape3Collection shapeCollection, Color color)
        {
            if (shapeCollection.ShapesEnabled == null)
            {
                for (int i = 0; i < shapeCollection.Shapes.Count; i++)
                {
                    Draw(shapeCollection.Shapes[i], color);
                }
            }
            else
            {
                for (int i = 0; i < shapeCollection.Shapes.Count; i++)
                {
                    if (shapeCollection.ShapesEnabled.Count <= i || shapeCollection.ShapesEnabled[i])
                        Draw(shapeCollection.Shapes[i], color);
                }
            }
        }

        // don't make public, cause this method is slower than directly drawing with the Instances* properties
        private void Draw(IShape3 shape, Color color)
        {
            switch (shape)
            {
                case AABB3 f: Boxes.Draw(f, color); break;
                case Axis3 f: Lines.Draw(f, color); break;
                case AxisRadius3 f: OpenCylinders.Draw(f, color); break;
                case Box3 f: Boxes.Draw(f, color); break;
                case Capsule3 f: Draw(f, color); break;
                case Cylinder3 f: Cylinders.Draw(f, color); break;
                case Line3 f: Lines.Draw(f, color); break;
                case Plane3 f: Triangles.Draw(f, color); break;
                case Point3 f: Lines.Draw(f, color, 0.1f); break;
                case Ray3 f: Lines.Draw(f, color); break;
                case Shape3Collection f: Draw(f, color); break;
                case Sphere3 f: Spheres.Draw(f, color); break;
                case Triangle3 f: Triangles.Draw(f, color); break;
                default: throw new NotImplementedException();
            }
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
