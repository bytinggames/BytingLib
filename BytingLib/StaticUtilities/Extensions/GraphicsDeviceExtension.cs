namespace BytingLib
{
    public static class GraphicsDeviceExtension
    {
        public static IDisposable UseRenderTarget(this GraphicsDevice gDevice, RenderTarget2D renderTarget)
        {
            return UseRenderTargets(gDevice, [new RenderTargetBinding(renderTarget)]);
        }

        public static IDisposable UseRenderTargets(this GraphicsDevice gDevice, params RenderTargetBinding[] bindings)
        {
            var rememberBindings = gDevice.GetRenderTargets();
            gDevice.SetRenderTargets(bindings);

            return new OnDispose(() => gDevice.SetRenderTargets(rememberBindings));
        }
    }
}
