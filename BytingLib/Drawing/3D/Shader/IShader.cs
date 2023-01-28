namespace BytingLib
{
    public interface IShader
    {
        Effect Effect { get; }

        void ApplyParameters();
        IDisposable UseTechnique(string technique);
        IDisposable UseRasterizer(RasterizerState rasterizerState);
        IDisposable UseSampler(SamplerState samplerState, int index = 0);
        IDisposable UseBlend(BlendState blendState);
        IDisposable UseDepthStencil(DepthStencilState depthStencilState);
        IDisposable UseScissorsRectangle(Rectangle scissorsRectangle);
    }
}
