namespace BytingLib
{
    public abstract class ShaderDefault1 : Shader, IShaderWorld, IShaderAlbedo
    {
        protected Matrix view, projection;

        public ShaderDefault1(Ref<Effect> effect)
            : base(effect)
        { }

        public abstract EffectParameterStack<Matrix> World { get; }
        public abstract EffectParameterStack<Texture2D> AlbedoTex { get; }

        #region Draw

        public void DrawFull(Matrix view, Matrix projection, Action draw)
        {
            this.view = view;
            this.projection = projection;

            DrawFullChild(draw);
        }

        protected virtual void DrawFullChild(Action draw)
        {
            draw();
        }

        //public void Draw(Model model)
        //{
        //    var e = effect.Value;

        //    foreach (var mesh in model.Meshes)
        //    {
        //        using (World.Use(f => mesh.ParentBone.Transform * f))
        //        {
        //            foreach (var part in mesh.MeshParts)
        //            {
        //                gDevice.Indices = part.IndexBuffer;

        //                var basicEffect = (part.Effect as BasicEffect)!;
        //                var texture = basicEffect.Texture;

        //                using (AlbedoTex.Use(texture))
        //                {
        //                    using (Apply(part.VertexBuffer))
        //                    {
        //                        foreach (var pass in e.CurrentTechnique.Passes)
        //                        {
        //                            pass.Apply();
        //                            gDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList,
        //                                part.VertexOffset, part.StartIndex, part.PrimitiveCount);
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}

        //public void DrawTriangles<V>(V[] vertices) where V : struct, IVertexType
        //{
        //    if (vertices.Length == 0)
        //        return;

        //    var e = effect.Value;

        //    using (Apply(vertices[0].VertexDeclaration))
        //    {
        //        foreach (var pass in e.CurrentTechnique.Passes)
        //        {
        //            pass.Apply();
        //            gDevice.DrawUserPrimitives(PrimitiveType.TriangleList, vertices, 0, vertices.Length / 3);
        //        }
        //    }
        //}
        /// <summary>only for testing currently</summary>
        public void Draw(VertexBuffer vertexBuffer, IndexBuffer indexBuffer)
        {
            var e = Effect.Value;

            gDevice.Indices = indexBuffer;
            using (Apply(vertexBuffer))
            {
                foreach (var pass in e.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    gDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList,
                        0, 0, indexBuffer.IndexCount / 3);
                }
            }
        }

        #endregion
    }
}
