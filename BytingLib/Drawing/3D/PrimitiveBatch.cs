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

        public void End(IShaderWorld shader)
        {
            if (!begun)
                throw new BytingException("Begin() has not yet been called.");

            try
            {
                DrawCustom(shader);

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

        protected virtual void DrawCustom(IShaderWorld shader)
        {
        }

        public void Dispose()
        {
            InstanceBuffer?.Dispose();

            instancesAndBuffers.Clear();
        }

        public void Draw(Axis3 axis, Color color) => Lines.Draw(axis, color);
        public void Draw(Line3 line, Color color) => Lines.Draw(line, color);
        public void Draw(Ray3 ray, Color color) => Lines.Draw(ray, color);
        public void Draw(Vector3 pos, Color color, float size) => Lines.Draw(pos, color, size);
        public void Draw(Point3 point, Color color, float size) => Lines.Draw(point, color, size);
        public void Draw(Plane3 plane, Color color) => Triangles.Draw(plane, color);
        public void Draw(Plane3 plane, Color color, float infinity) => Triangles.Draw(plane, color, infinity);
        public void Draw(Plane3 plane, Color color, float infinity, Vector3 axis1) => Triangles.Draw(plane, color, infinity, axis1);
        public void Draw(Triangle3 triangle, Color color) => Triangles.Draw(triangle, color);
        public void Draw(AABB3 aabb, Color color) => Boxes.Draw(aabb, color);
        public void Draw(Box3 box, Color color) => Boxes.Draw(box, color);
        public void Draw(Sphere3 sphere, Color color) => Spheres.Draw(sphere, color);
        public void Draw(AxisRadius3 axisRadius, Color color) => OpenCylinders.Draw(axisRadius, color);
        public void Draw(AxisRadius3 axisRadius, Color color, Vector3 length) => OpenCylinders.Draw(axisRadius, color, length);
        public void DrawOpenCylinder(Cylinder3 cylinder, Color color) => OpenCylinders.Draw(cylinder, color);
        public void Draw(Cylinder3 cylinder, Color color) => Cylinders.Draw(cylinder, color);

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
                    Draw(shapeCollection.Shapes[i], color);
            }
            else
            {
                for (int i = 0; i < shapeCollection.Shapes.Count; i++)
                    if (shapeCollection.ShapesEnabled.Count <= i || shapeCollection.ShapesEnabled[i])
                        Draw(shapeCollection.Shapes[i], color);
            }
        }

        /// <summary>If you know the type of the shape, consider using the direct draw call to prevent a cast.</summary>
        public void Draw(IShape3 shape, Color color)
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

        public void DrawCage(Vector3 pos, Vector3 size, Color color)
        {
            Vector3 length = new Vector3(size.X, 0, 0);

            Lines.Draw(pos, length, color);
            Lines.Draw(pos + new Vector3(0, size.Y, 0), length, color);
            Lines.Draw(pos + new Vector3(0, size.Y, size.Z), length, color);
            Lines.Draw(pos + new Vector3(0, 0, size.Z), length, color);

            length = new Vector3(0, size.Y, 0);
            Lines.Draw(pos, length, color);
            Lines.Draw(pos + new Vector3(size.X, 0, 0), length, color);
            Lines.Draw(pos + new Vector3(size.X, 0, size.Z), length, color);
            Lines.Draw(pos + new Vector3(0, 0, size.Z), length, color);

            length = new Vector3(0, 0, size.Z);
            Lines.Draw(pos, length, color);
            Lines.Draw(pos + new Vector3(size.X, 0, 0), length, color);
            Lines.Draw(pos + new Vector3(size.X, size.Y, 0), length, color);
            Lines.Draw(pos + new Vector3(0, size.Y, 0), length, color);
        }

        public void Draw(BoundingFrustum frustum, Color color)
        {
            var corners = frustum.GetCorners();

            // side planes
            for (int i = 0; i < 4; i++)
            {
                int i2 = (i + 1) % 4;
                Triangles.Draw(corners[i], corners[4 + i], corners[i2], color);
                Triangles.Draw(corners[4 + i], corners[4 + i2], corners[i2], color);
            }
            // near plane
            Triangles.Draw(corners[0], corners[1], corners[2], color);
            Triangles.Draw(corners[0], corners[2], corners[3], color);

            // far plane
            Triangles.Draw(corners[4], corners[6], corners[5], color);
            Triangles.Draw(corners[4], corners[7], corners[6], color);
        }

        public void DrawCage(BoundingFrustum frustum, Color color)
        {
            var corners = frustum.GetCorners();
            DrawCageFrom8Corners(corners, color);
        }

        public void DrawCage(Box3 box, Color color)
        {
            var corners = box.GetCorners();
            DrawCageFrom8Corners(corners, color);
        }

        private void DrawCageFrom8Corners(IList<Vector3> corners, Color color)
        {
            // side lines
            for (int i = 0; i < 4; i++)
                Lines.Draw(corners[i], corners[4 + i] - corners[i], color);

            // near and far plane lines
            for (int i = 0; i < 4; i++)
            {
                Lines.Draw(corners[i], corners[(i + 1) % 4] - corners[i], color);
                Lines.Draw(corners[4 + i], corners[4 + (i + 1) % 4] - corners[4 + i], color);
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
