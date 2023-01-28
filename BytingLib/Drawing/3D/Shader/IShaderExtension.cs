namespace BytingLib
{
    public static class IShaderExtension
    {
        /// <summary>only for testing currently</summary>
        public static void Draw(this IShader shader, VertexBuffer vertexBuffer, IndexBuffer indexBuffer)
        {
            var e = shader.Effect;
            var gDevice = vertexBuffer.GraphicsDevice;

            gDevice.SetVertexBuffer(vertexBuffer);
            gDevice.Indices = indexBuffer;

            shader.ApplyParameters();
            foreach (var pass in e.CurrentTechnique.Passes)
            {
                pass.Apply();
                gDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList,
                    0, 0, indexBuffer.IndexCount / 3);
            }
        }
    }
}
