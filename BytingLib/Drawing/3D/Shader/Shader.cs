namespace BytingLib
{
    public abstract class Shader : IShader, IDisposable
    {
        /// <summary>To change the current technique, use UseTechnique()</summary>
        public Ref<Effect> Effect { get; private set; }
        protected readonly GraphicsDevice gDevice;
        protected List<IEffectParameterStack> parameters = new();
        protected string currentTechnique;

        protected virtual string TechniqueNonInstanced => "Render";
        protected virtual string TechniqueInstanced => "RenderInstanced";

        public Shader(Ref<Effect> effect)
        {
            Effect = effect;
            gDevice = effect.Value.GraphicsDevice;
            currentTechnique = effect.Value.CurrentTechnique.Name;
        }

        public void Dispose()
        {
            foreach (var p in parameters)
            {
                p.Dispose();
            }
        }

        protected void AddParam(IEffectParameterStack parameter) => parameters.Add(parameter);

        /// <summary>
        /// Used for calling Initialize from generated c# shader code
        /// </summary>
        protected virtual void Initialize() { }

        #region Apply

        public void ApplyParameters()
        {
            // actually apply the current technique
            Effect.Value.CurrentTechnique = Effect.Value.Techniques[currentTechnique];

            for (int i = 0; i < parameters.Count; i++)
            {
                parameters[i].Apply();
            }
        }

        public void Apply(DisposableContainer disposables, VertexBuffer vertexBuffer)
        {
            gDevice.SetVertexBuffer(vertexBuffer);

            UseVertexDeclaration(disposables, vertexBuffer.VertexDeclaration);
            ApplyParameters(disposables, false);
        }

        public void Apply(DisposableContainer disposables, VertexBufferBinding[] vertexBufferBindings)
        {
            gDevice.SetVertexBuffers(vertexBufferBindings);

            UseVertexDeclaration(disposables, vertexBufferBindings[0].VertexBuffer.VertexDeclaration);
            ApplyParameters(disposables, vertexBufferBindings.Length > 1);
        }

        /// <summary>Used, when not rendering from a VertexBuffer</summary>
        public void Apply(DisposableContainer disposables, VertexDeclaration vertexDeclaration)
        {
            UseVertexDeclaration(disposables, vertexDeclaration);
            ApplyParameters(disposables, false);
        }

        private void ApplyParameters(DisposableContainer disposables, bool instanced)
        {
            ApplyParametersInner(instanced, disposables);

            ApplyParameters();
        }

        protected virtual void ApplyParametersInner(bool instanced, DisposableContainer disposables)
        {
            if (instanced)
            {
                disposables.UseCheckNull(UseTechnique(TechniqueInstanced));
                UseInstancedRender(disposables);
            }
            else
            {
                disposables.UseCheckNull(UseTechnique(TechniqueNonInstanced));
            }
        }

        #endregion

        #region Use

        public virtual void UseMaterial(DisposableContainer disposables, MaterialGL material) { }
        protected virtual void UseVertexDeclaration(DisposableContainer disposables, VertexDeclaration vertexDeclaration) { }
        protected virtual void UseInstancedRender(DisposableContainer disposables) { }

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
        public IDisposable? UseDepthStencil(DepthStencilState depthStencilState)
        {
            if (depthStencilState == gDevice.DepthStencilState)
            {
                return null; // otherwise this causes an exception
            }

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
    }
}
