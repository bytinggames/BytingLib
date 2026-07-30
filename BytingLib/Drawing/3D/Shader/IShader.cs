namespace BytingLib
{
    public interface IShader
    {
        Ref<Effect> Effect { get; }

        void Apply(DisposableContainer disposables, params VertexBufferBinding[] vertexBufferBindings);
        void Apply(DisposableContainer disposables, VertexBuffer vertexBuffer);
        void Apply(DisposableContainer disposables, VertexDeclaration vertexDeclaration);
        void ApplyParameters();
        IDisposable UseTechnique(string technique);
        IDisposable UseRasterizer(RasterizerState rasterizerState);
        IDisposable UseSampler(SamplerState samplerState, int index = 0);
        IDisposable UseBlend(BlendState blendState);
        IDisposable? UseDepthStencil(DepthStencilState depthStencilState);
        IDisposable UseScissorsRectangle(Rectangle scissorsRectangle);
    }
}
