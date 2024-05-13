namespace BytingLib
{
    public static class IShaderExtension
    {

        public static void Draw(this IShader shader, VertexBuffer vertexBuffer)
        {
            var e = shader.Effect.Value;
            var gDevice = vertexBuffer.GraphicsDevice;

            using (shader.Apply(vertexBuffer))
            {
                foreach (var pass in e.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    gDevice.DrawPrimitives(PrimitiveType.TriangleList,
                        0, 0);
                }
            }
        }

        public static void Draw(this IShader shader, VertexBuffer vertexBuffer, IndexBuffer indexBuffer, PrimitiveType primitiveType = PrimitiveType.TriangleList)
        {
            var e = shader.Effect.Value;
            var gDevice = vertexBuffer.GraphicsDevice;

            gDevice.Indices = indexBuffer;

            using (shader.Apply(vertexBuffer))
            {
                foreach (var pass in e.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    gDevice.DrawIndexedPrimitives(primitiveType,
                        0, 0, primitiveType.GetPrimitiveCount(indexBuffer.IndexCount));
                }
            }
        }

        public static void DrawTriangles<V>(this IShader shader, V[] vertices) where V : struct, IVertexType
        {
            if (vertices.Length == 0)
            {
                return;
            }

            var e = shader.Effect.Value;

            using (shader.Apply(vertices[0].VertexDeclaration))
            {
                foreach (var pass in e.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    e.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, vertices, 0, vertices.Length / 3);
                }
            }
        }


        public static void Draw(this IShaderWorld shaderWorld, Model model, IShaderAlbedo? shaderAlbedo)
        {
            var gDevice = shaderWorld.Effect.Value.GraphicsDevice;

            foreach (var mesh in model.Meshes)
            {
                using (shaderWorld.World.Use(f => mesh.ParentBone.Transform * f))
                {
                    foreach (var part in mesh.MeshParts)
                    {
                        gDevice.Indices = part.IndexBuffer;

                        IDisposable? textureUsage = null;
                        if (shaderAlbedo != null)
                        {
                            var basicEffect = (part.Effect as BasicEffect)!;
                            var texture = basicEffect.Texture;
                            textureUsage = shaderAlbedo.AlbedoTex.Use(texture);
                        }
                        using (textureUsage)
                        {
                            using (shaderWorld.Apply(part.VertexBuffer))
                            {
                                foreach (var pass in shaderWorld.Effect.Value.CurrentTechnique.Passes)
                                {
                                    pass.Apply();
                                    gDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList,
                                        part.VertexOffset, part.StartIndex, part.PrimitiveCount);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
