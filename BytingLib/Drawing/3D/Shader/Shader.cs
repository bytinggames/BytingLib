namespace BytingLib
{
    public abstract class Shader : IShaderWorld, IShaderAlbedo, IDisposable
    {
        /// <summary>To change the current technique, use UseTechnique()</summary>
        protected readonly Ref<Effect> effect;
        protected readonly GraphicsDevice gDevice;
        protected Matrix view, projection;
        private string currentTechnique;

        protected List<IEffectParameterStack> parameters = new();

        protected void Add(IEffectParameterStack parameter) => parameters.Add(parameter);

        public Shader(Ref<Effect> effect)
        {
            this.effect = effect;
            gDevice = effect.Value.GraphicsDevice;
            currentTechnique = effect.Value.CurrentTechnique.Name;
        }


        public Effect Effect => effect.Value;

        public abstract EffectParameterStack<Matrix> World { get; }
        public abstract EffectParameterStack<Texture2D> AlbedoTex { get; }

        #region Apply

        public IDisposable Apply(VertexBuffer vertexBuffer)
        {
            gDevice.SetVertexBuffer(vertexBuffer);

            DisposableContainer disposables = new();
            disposables.Use(UseVertexDeclaration(vertexBuffer.VertexDeclaration));
            disposables.UseCheckNull(ApplyParameters(false));

            return disposables;
        }

        public IDisposable Apply(VertexBufferBinding[] vertexBufferBindings)
        {
            gDevice.SetVertexBuffers(vertexBufferBindings);

            DisposableContainer disposables = new();
            disposables.Use(UseVertexDeclaration(vertexBufferBindings[0].VertexBuffer.VertexDeclaration));
            disposables.UseCheckNull(ApplyParameters(vertexBufferBindings.Length > 1));

            return disposables;
        }

        /// <summary>Used, when not rendering from a VertexBuffer</summary>
        public IDisposable Apply(VertexDeclaration vertexDeclaration)
        {
            DisposableContainer disposables = new();
            disposables.Use(UseVertexDeclaration(vertexDeclaration));
            disposables.UseCheckNull(ApplyParameters(false));

            return disposables;
        }

        private IDisposable ApplyParameters(bool instanced)
        {
            DisposableContainer disposables = new();

            if (instanced)
            {
                disposables.UseCheckNull(UseTechnique("RenderInstanced"));
                disposables.UseCheckNull(UseInstancedRender());
            }
            else
                disposables.UseCheckNull(UseTechnique("Render"));

            // actually apply the current technique
            effect.Value.CurrentTechnique = effect.Value.Techniques[currentTechnique];

            for (int i = 0; i < parameters.Count; i++)
            {
                parameters[i].Apply();
            }

            return disposables;
        }

        #endregion

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

        public void Draw(Model model)
        {
            var e = effect.Value;

            foreach (var mesh in model.Meshes)
            {
                using (World.Use(f => mesh.ParentBone.Transform * f))
                {
                    foreach (var part in mesh.MeshParts)
                    {
                        gDevice.Indices = part.IndexBuffer;

                        var basicEffect = (part.Effect as BasicEffect)!;
                        var texture = basicEffect.Texture;

                        using (AlbedoTex.Use(texture))
                        {
                            using (Apply(part.VertexBuffer))
                            {
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
        }

        public void DrawTriangles<V>(V[] vertices) where V : struct, IVertexType
        {
            if (vertices.Length == 0)
                return;

            var e = effect.Value;

            using (Apply(vertices[0].VertexDeclaration))
            {
                foreach (var pass in e.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    gDevice.DrawUserPrimitives(PrimitiveType.TriangleList, vertices, 0, vertices.Length / 3);
                }
            }
        }
        /// <summary>only for testing currently</summary>
        public void Draw(VertexBuffer vertexBuffer, IndexBuffer indexBuffer)
        {
            var e = effect.Value;

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

        #region Use

        public abstract IDisposable UseMaterial(MaterialGL material);
        protected abstract IDisposable UseVertexDeclaration(VertexDeclaration vertexDeclaration);
        protected abstract IDisposable? UseInstancedRender();

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

        #endregion

        public void Dispose()
        {
            foreach (var p in parameters)
                p.Dispose();
        }
    }
}
