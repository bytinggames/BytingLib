namespace BytingLib
{
    public abstract class Shader : IShaderWorld, IShaderColorTex, IDisposable
    {
        /// <summary>To change the current technique, use UseTechnique()</summary>
        protected readonly Ref<Effect> effect;
        protected readonly GraphicsDevice gDevice;
        protected Matrix view, projection;
        private string currentTechnique;

        protected List<IEffectParameterStack> parameters = new();

        protected void Add(IEffectParameterStack parameter) => parameters.Add(parameter);

        public abstract EffectParameterStack<Matrix> World { get; }
        public abstract EffectParameterStack<Texture2D> ColorTex { get; }

        public Effect Effect => effect.Value;

        public Shader(Ref<Effect> effect)
        {
            this.effect = effect;
            gDevice = effect.Value.GraphicsDevice;
            currentTechnique = effect.Value.CurrentTechnique.Name;
        }

        public void ApplyParameters()
        {
            effect.Value.CurrentTechnique = effect.Value.Techniques[currentTechnique];

            for (int i = 0; i < parameters.Count; i++)
            {
                parameters[i].Apply();
            }
        }

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

        public void Draw(Model model)
        {
            var e = effect.Value;

            foreach (var mesh in model.Meshes)
            {
                using (World.Use(f => mesh.ParentBone.Transform * f))
                {
                    foreach (var part in mesh.MeshParts)
                    {
                        gDevice.SetVertexBuffer(part.VertexBuffer);
                        gDevice.Indices = part.IndexBuffer;

                        var basicEffect = (part.Effect as BasicEffect)!;
                        var texture = basicEffect.Texture;

                        string techniqueName = GetTechniqueName(part.VertexBuffer.VertexDeclaration);

                        using (UseTechnique(techniqueName))
                        using (ColorTex.Use(texture))
                        {
                            ApplyParameters();
                            foreach (var pass in e.CurrentTechnique.Passes)
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

        public static string GetTechniqueName(VertexDeclaration vertexDeclaration)
        {
            string techniqueName = "Render";

            var elements = vertexDeclaration.GetVertexElements();

            if (elements.Any(f => f.VertexElementUsage == VertexElementUsage.Color))
                techniqueName += "Color";
            if (elements.Any(f => f.VertexElementUsage == VertexElementUsage.Normal))
                techniqueName += "Normal";
            if (elements.Any(f => f.VertexElementUsage == VertexElementUsage.TextureCoordinate))
                techniqueName += "Texture";
            if (elements.Any(f => f.VertexElementUsage == VertexElementUsage.BlendIndices)
                && elements.Any(f => f.VertexElementUsage == VertexElementUsage.BlendWeight))
                techniqueName += "Skinned";

            return techniqueName;
        }

        public void DrawTriangles<V>(V[] vertices) where V : struct, IVertexType
        {
            var e = effect.Value;

            string vertexName = typeof(V).Name;
            string technique = "Render" + vertexName.Substring("VertexPosition".Length);

            using (UseTechnique(technique))
            {
                ApplyParameters();
                foreach (var pass in e.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    gDevice.DrawUserPrimitives(PrimitiveType.TriangleList, vertices, 0, vertices.Length / 3);
                }
            }
        }

        public IDisposable UseRasterizer(RasterizerState rasterizerState)
        {
            var storeVal = gDevice.RasterizerState;
            gDevice.RasterizerState = rasterizerState;

            return new OnDispose(() => gDevice.RasterizerState = storeVal);
        }
        public IDisposable UseSampler(SamplerState samplerState, int index = 0)
        {
            var storeVal = gDevice.SamplerStates[index];
            gDevice.SamplerStates[index] = samplerState;

            return new OnDispose(() => gDevice.SamplerStates[index] = storeVal);
        }
        public IDisposable UseBlend(BlendState blendState)
        {
            var storeVal = gDevice.BlendState;
            gDevice.BlendState = blendState;

            return new OnDispose(() => gDevice.BlendState = storeVal);
        }
        public IDisposable UseDepthStencil(DepthStencilState depthStencilState)
        {
            var storeVal = gDevice.DepthStencilState;
            gDevice.DepthStencilState = depthStencilState;

            return new OnDispose(() => gDevice.DepthStencilState = storeVal);
        }
        public IDisposable UseScissorsRectangle(Rectangle scissorsRectangle)
        {
            var storeVal = gDevice.ScissorRectangle;
            gDevice.ScissorRectangle = scissorsRectangle;

            return new OnDispose(() => gDevice.ScissorRectangle = storeVal);
        }

        public IDisposable UseTechnique(string technique)
        {
            var storeTechnique = currentTechnique;
            currentTechnique = technique; // gets applied on ApplyParameters()

            return new OnDispose(() => currentTechnique = storeTechnique);
        }

        public void Dispose()
        {
            foreach (var p in parameters)
                p.Dispose();
        }




        /// <summary>only for testing currently</summary>
        public void Draw(VertexBuffer vertexBuffer, IndexBuffer indexBuffer)
        {
            var e = effect.Value;

            gDevice.SetVertexBuffer(vertexBuffer);
            gDevice.Indices = indexBuffer;

            string techniqueName = GetTechniqueName(vertexBuffer.VertexDeclaration);

            using (UseTechnique(techniqueName))
            {
                ApplyParameters();
                foreach (var pass in e.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    gDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList,
                        0, 0, indexBuffer.IndexCount / 3);
                }
            }
        }
    }
}
