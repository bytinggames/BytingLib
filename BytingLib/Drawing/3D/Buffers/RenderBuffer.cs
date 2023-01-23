using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BytingLib
{

    public abstract class RenderBuffer<V> : IDisposable, IRenderBuffer where V : struct, IVertexType
    {
        VertexBuffer VertexBuffer;
        IndexBuffer IndexBuffer;
        SimpleArrayList<VertexInstanceTransformColor> Instances = new();
        private readonly GraphicsDevice gDevice;

        public RenderBuffer(GraphicsDevice gDevice, V[] vertices, short[] indices)
        {
            VertexBuffer = new(gDevice, vertices.GetType().GetElementType(), vertices.Length, BufferUsage.WriteOnly);
            VertexBuffer.SetData(vertices);

            IndexBuffer = new IndexBuffer(gDevice, IndexElementSize.SixteenBits, indices.Length, BufferUsage.WriteOnly);
            IndexBuffer.SetData(indices);
            this.gDevice = gDevice;
        }

        public void Add(Matrix transform, Color color)
        {
            Instances.Add(new VertexInstanceTransformColor(transform, color));
        }

        public void Clear()
        {
            Instances.Clear();
        }

        public int Count => Instances.Count;

        public abstract bool HasNormal { get; }

        public void Draw(DynamicVertexBuffer instanceBuffer, Effect effect)
        {
            effect.CurrentTechnique = effect.Techniques["Render" + (HasNormal ? "Normal" : "") + "Instanced"];

            instanceBuffer.SetData(Instances.Arr, 0, Instances.Count, SetDataOptions.Discard);

            gDevice.SetVertexBuffers(
                new VertexBufferBinding(VertexBuffer),
                new VertexBufferBinding(instanceBuffer, 0, 1)
                );
            gDevice.Indices = IndexBuffer;

            foreach (var pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                if (HasNormal) // triangles
                    gDevice.DrawInstancedPrimitives(PrimitiveType.TriangleList, 0, 0, VertexBuffer.VertexCount / 3, Instances.Count);
                else // lines
                    gDevice.DrawInstancedPrimitives(PrimitiveType.LineList, 0, 0, VertexBuffer.VertexCount / 2, Instances.Count);
            }

            Clear();
        }

        public void Draw(DynamicVertexBuffer instanceBuffer, IShader shader)
        {
            shader.ApplyParameters();
            Draw(instanceBuffer, shader.Effect);
        }

        public void Dispose()
        {
            VertexBuffer.Dispose();
            IndexBuffer.Dispose();
        }
    }
}
