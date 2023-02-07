using BytingLib.StaticUtilities.Extensions;

namespace BytingLib
{
    public class InstanceDrawer<InstanceVertex> where InstanceVertex : struct, IVertexType
    {
        private static void DrawBegin(IInstances<InstanceVertex> instances, DynamicVertexBuffer instanceBuffer, Effect effect)
        {
            if (instances.Count == 0)
                return;

            instanceBuffer.SetData(instances.Array, 0, instances.Count, SetDataOptions.Discard);
        }

        private static void DrawEnd(IInstances<InstanceVertex> instances)
        {
            instances.Clear();
        }

        public static void DrawBuffers(IShader shader, IInstances<InstanceVertex> instances, DynamicVertexBuffer instanceBuffer,
            VertexBuffer vertexBuffer, IndexBuffer indexBuffer, PrimitiveType primitiveType)
        {
            DrawBegin(instances, instanceBuffer, shader.Effect);

            int primitiveCount = primitiveType.GetPrimitiveCount(indexBuffer.IndexCount);

            DrawInstancesInner(shader, instances, instanceBuffer, vertexBuffer, indexBuffer, 0, 0, primitiveCount, primitiveType);

            DrawEnd(instances);
        }

        public static void DrawBuffers(IShader shader, IInstances<InstanceVertex> instances, DynamicVertexBuffer instanceBuffer,
            VertexIndexBuffer buffer)
        {
            DrawBuffers(shader, instances, instanceBuffer, buffer.VertexBuffer, buffer.IndexBuffer, buffer.PrimitiveType);
        }

        public static void DrawBuffers(IShader shader, IInstances<InstanceVertex> instances, DynamicVertexBuffer instanceBuffer, 
            VertexBuffer vertexBuffer, IndexBuffer indexBuffer, int vertexOffset, int indexOffset, int primitiveCount, PrimitiveType primitiveType)
        {
            DrawBegin(instances, instanceBuffer, shader.Effect);

            DrawInstancesInner(shader, instances, instanceBuffer, vertexBuffer, indexBuffer, vertexOffset, indexOffset, primitiveCount, primitiveType);

            DrawEnd(instances);
        }

        public static void DrawMeshPart(IShader shader, IInstances<InstanceVertex> instances, DynamicVertexBuffer instanceBuffer, ModelMeshPart meshPart)
        {
            DrawBuffers(shader, instances, instanceBuffer, meshPart.VertexBuffer, meshPart.IndexBuffer, meshPart.VertexOffset, meshPart.StartIndex, meshPart.PrimitiveCount, PrimitiveType.TriangleList);
        }

        public static void DrawMesh<ShaderColorTex>(ShaderColorTex shader, IInstances<InstanceVertex> instances, DynamicVertexBuffer instanceBuffer, ModelMesh mesh)
            where ShaderColorTex : IShaderAlbedo, IShaderWorld
        {
            DrawBegin(instances, instanceBuffer, shader.Effect);

            DrawInstancesInner(shader, instances, instanceBuffer, mesh);

            DrawEnd(instances);
        }

        public static void DrawModel<ShaderColorTex>(ShaderColorTex shader, IInstances<InstanceVertex> instances, DynamicVertexBuffer instanceBuffer, Model model)
            where ShaderColorTex : IShaderAlbedo, IShaderWorld
        {
            DrawBegin(instances, instanceBuffer, shader.Effect);

            foreach (var mesh in model.Meshes)
            {
                DrawInstancesInner(shader, instances, instanceBuffer, mesh);
            }

            DrawEnd(instances);
        }


        private static void DrawInstancesInner<ShaderColorTex>(ShaderColorTex shader, IInstances<InstanceVertex> instances, DynamicVertexBuffer instanceBuffer, ModelMesh mesh)
            where ShaderColorTex : IShaderAlbedo, IShaderWorld
        {
            using (shader.World.Use(mesh.ParentBone.Transform))
            {
                foreach (var part in mesh.MeshParts)
                {
                    using (shader.AlbedoTex.Use(((BasicEffect)part.Effect).Texture))
                    {
                        DrawInstancesInner(shader, instances, instanceBuffer, part.VertexBuffer, part.IndexBuffer, part.VertexOffset,
                            part.StartIndex, part.PrimitiveCount, PrimitiveType.TriangleList);
                    }
                }
            }
        }

        private static void DrawInstancesInner(IShader shader, IInstances<InstanceVertex> instances, DynamicVertexBuffer instanceBuffer, 
            VertexBuffer vertexBuffer, IndexBuffer indexBuffer, int vertexOffset, int indexOffset, int primitiveCount,
            PrimitiveType primitiveType)
        {
            var gDevice = instanceBuffer.GraphicsDevice;

            gDevice.Indices = indexBuffer;
            using (shader.Apply(
                new VertexBufferBinding(vertexBuffer, vertexOffset),
                new VertexBufferBinding(instanceBuffer, 0, 1)))
            {
                foreach (var pass in shader.Effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    gDevice.DrawInstancedPrimitives(primitiveType, 0, indexOffset, primitiveCount, instances.Count);
                }
            }
        }
    }
}
