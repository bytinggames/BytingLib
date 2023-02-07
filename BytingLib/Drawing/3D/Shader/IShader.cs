namespace BytingLib
{
    public interface IShader
    {
        Effect Effect { get; }

        IDisposable Apply(params VertexBufferBinding[] vertexBufferBindings);
        IDisposable Apply(VertexBuffer vertexBuffer);
        IDisposable Apply(VertexDeclaration vertexDeclaration);
        IDisposable UseTechnique(string technique);
        IDisposable UseRasterizer(RasterizerState rasterizerState);
        IDisposable UseSampler(SamplerState samplerState, int index = 0);
        IDisposable UseBlend(BlendState blendState);
        IDisposable UseDepthStencil(DepthStencilState depthStencilState);
        IDisposable UseScissorsRectangle(Rectangle scissorsRectangle);
    }
}
