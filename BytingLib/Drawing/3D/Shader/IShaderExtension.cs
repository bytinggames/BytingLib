namespace BytingLib
{
    public static class IShaderExtension
    {
        public static void Draw(this IShader shader, VertexBuffer vertexBuffer, IndexBuffer indexBuffer)
        {
            var e = shader.Effect;
            var gDevice = vertexBuffer.GraphicsDevice;

            gDevice.Indices = indexBuffer;

            using (shader.Apply(vertexBuffer))
            {
                foreach (var pass in e.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    gDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList,
                        0, 0, indexBuffer.IndexCount / 3);
                }
            }
        }

        public static void Draw(this IShader shader, VertexBuffer vertexBuffer)
        {
            var e = shader.Effect;
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
    }
}
