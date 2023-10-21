namespace BytingLib
{
    public abstract class Shader : IShader, IDisposable
    {
        /// <summary>To change the current technique, use UseTechnique()</summary>
        protected readonly Ref<Effect> effect;
        protected readonly GraphicsDevice gDevice;
        protected List<IEffectParameterStack> parameters = new();
        private string currentTechnique;

        protected virtual string TechniqueNonInstanced => "Render";
        protected virtual string TechniqueInstanced => "RenderInstanced";

        public Shader(Ref<Effect> effect)
        {
            this.effect = effect;
            gDevice = effect.Value.GraphicsDevice;
            currentTechnique = effect.Value.CurrentTechnique.Name;
        }

        public Effect Effect => effect.Value;

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
            effect.Value.CurrentTechnique = effect.Value.Techniques[currentTechnique];

            for (int i = 0; i < parameters.Count; i++)
            {
                parameters[i].Apply();
            }
        }

        public IDisposable Apply(VertexBuffer vertexBuffer)
        {
            gDevice.SetVertexBuffer(vertexBuffer);

            DisposableContainer disposables = new();
            disposables.UseCheckNull(UseVertexDeclaration(vertexBuffer.VertexDeclaration));
            disposables.UseCheckNull(ApplyParameters(false));

            return disposables;
        }

        public IDisposable Apply(VertexBufferBinding[] vertexBufferBindings)
        {
            gDevice.SetVertexBuffers(vertexBufferBindings);

            DisposableContainer disposables = new();
            disposables.UseCheckNull(UseVertexDeclaration(vertexBufferBindings[0].VertexBuffer.VertexDeclaration));
            disposables.UseCheckNull(ApplyParameters(vertexBufferBindings.Length > 1));

            return disposables;
        }

        /// <summary>Used, when not rendering from a VertexBuffer</summary>
        public IDisposable Apply(VertexDeclaration vertexDeclaration)
        {
            DisposableContainer disposables = new();
            disposables.UseCheckNull(UseVertexDeclaration(vertexDeclaration));
            disposables.UseCheckNull(ApplyParameters(false));

            return disposables;
        }

        private IDisposable ApplyParameters(bool instanced)
        {
            DisposableContainer disposables = new();

            if (instanced)
            {
                disposables.UseCheckNull(UseTechnique(TechniqueInstanced));
                disposables.UseCheckNull(UseInstancedRender());
            }
            else
            {
                disposables.UseCheckNull(UseTechnique(TechniqueNonInstanced));
            }

            ApplyParameters();

            return disposables;
        }

        #endregion

        #region Use

        public virtual IDisposable? UseMaterial(MaterialGL material) => null;
        protected virtual IDisposable? UseVertexDeclaration(VertexDeclaration vertexDeclaration) => null;
        protected virtual IDisposable? UseInstancedRender() => null;

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
    }
}
