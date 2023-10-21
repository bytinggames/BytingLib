namespace BytingLib
{
    public class TextureQuadRenderer
    {
        private readonly VertexBuffer vertexBuffer;
        private readonly IndexBuffer indexBuffer;

        private Rect? sourceRectUV; // null means rendering full texture

        public TextureQuadRenderer(GraphicsDevice gDevice)
        {
            VertexPositionNormalTexture[] renderVertices = new VertexPositionNormalTexture[]
            {
                new VertexPositionNormalTexture(new Vector3(-1,-1,0), Vector3.UnitZ, Vector2.UnitY),
                new VertexPositionNormalTexture(new Vector3(-1,1,0), Vector3.UnitZ, Vector2.Zero),
                new VertexPositionNormalTexture(new Vector3(1,-1,0), Vector3.UnitZ, Vector2.One),
                new VertexPositionNormalTexture(new Vector3(1,1,0), Vector3.UnitZ, Vector2.UnitX)
            };

            vertexBuffer = new VertexBuffer(gDevice, renderVertices[0].GetType(), renderVertices.Length, BufferUsage.WriteOnly);
            vertexBuffer.SetData(renderVertices);

            indexBuffer = new IndexBuffer(gDevice, IndexElementSize.SixteenBits, 6, BufferUsage.None);
            indexBuffer.SetData(new short[]
            {
                0,2,1,2,1,3
            });

            sourceRectUV = null;
        }

        private void Draw<S>(S shader, Matrix world, Texture2D texture, Rectangle? sourceRect = null)
            where S : IShaderWorld, IShaderAlbedo
        {
            UpdateSourceRectUV(texture, sourceRect);

            using (shader.World.Use(world))
            using (shader.AlbedoTex.Use(texture))
            {
                shader.Draw(vertexBuffer, indexBuffer, PrimitiveType.TriangleStrip);
            }
        }

        private void UpdateSourceRectUV(Texture2D texture, Rectangle? sourceRect)
        {
            Rect? newSourceRectUV = null;
            if (sourceRect.HasValue)
            {
                newSourceRectUV = new Rect((float)sourceRect.Value.X / texture.Width,
                    (float)sourceRect.Value.Y / texture.Height,
                    (float)sourceRect.Value.Width / texture.Width,
                    (float)sourceRect.Value.Height / texture.Height);
            }

            if (!newSourceRectUV.EqualValue(sourceRectUV))
            {
                sourceRectUV = newSourceRectUV;

                // source rect changed. Apply the rectangle coordinates to the vertex buffer
                Vector2[] uvs;
                if (newSourceRectUV == null)
                {
                    uvs = new Vector2[]
                    {
                        Vector2.UnitY,
                        Vector2.Zero,
                        Vector2.One,
                        Vector2.UnitX
                    };
                }
                else
                {
                    uvs = new Vector2[]
                    {
                        newSourceRectUV.BottomLeft,
                        newSourceRectUV.TopLeft,
                        newSourceRectUV.BottomRight,
                        newSourceRectUV.TopRight
                    };
                }
                int uvOffset = vertexBuffer.VertexDeclaration.GetVertexElements()
                    .First(f => f.VertexElementUsage == VertexElementUsage.TextureCoordinate)
                    .Offset;
                vertexBuffer.SetData(uvOffset, uvs, 0, uvs.Length, vertexBuffer.VertexDeclaration.VertexStride);
            }
        }

        public void DrawKeepAspectRatio<S>(S shader, Vector3 center, Vector3 right, Vector3 up, Texture2D texture, Rectangle? sourceRect = null)
            where S : IShaderWorld, IShaderAlbedo
        {
            int renderW = sourceRect == null ? texture.Width : sourceRect.Value.Width;
            int renderH = sourceRect == null ? texture.Height : sourceRect.Value.Height;
            ModifySizeToAspectRatio(ref right, ref up, renderW, renderH);

            DrawStretch(shader, center, right, up, texture, sourceRect);
        }

        public void DrawStretch<S>(S shader, Vector3 center, Vector3 right, Vector3 up, Texture2D texture, Rectangle? sourceRect = null)
            where S : IShaderWorld, IShaderAlbedo
        {
            Matrix world = CalculateTransform(center, right, up);

            Draw(shader, world, texture, sourceRect);
        }

        private void ModifySizeToAspectRatio(ref Vector3 right, ref Vector3 up, int texRenderWidth, int texRenderHeight)
        {
            float textureRatio = (float)texRenderWidth / texRenderHeight;
            float renderRatio = right.Length() / up.Length();

            if (renderRatio > textureRatio)
            {
                float widthFactor = renderRatio / textureRatio;
                right /= widthFactor;
            }
            else if (renderRatio < textureRatio)
            {
                float heightFactor = textureRatio / renderRatio;
                up /= heightFactor;
            }

            // apply stretching in y-direction
            up /= renderRatio;
        }

        private Matrix CalculateTransform(Vector3 center, Vector3 right, Vector3 up)
        {
            Vector3 x = Vector3.Normalize(right);
            Vector3 y = Vector3.Normalize(up);
            Vector3 z = Vector3.Cross(x, y);
            Matrix rotation = new Matrix(new Vector4(x, 0f), new Vector4(y, 0f), new Vector4(z, 0f), Vector4.UnitW);
            Matrix world = Matrix.CreateScale(right.Length(), up.Length(), 1f) * rotation * Matrix.CreateTranslation(center);
            return world;
        }
    }
}
