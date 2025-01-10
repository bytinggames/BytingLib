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
            try
            {
                gDevice.SetRenderTargets(bindings);
            }
            catch (InvalidOperationException)
            {
                // somehow sometimes this throws an error that not all bound images have the same sample count. Although they should.
                throw;
            }

            return new OnDispose(() => gDevice.SetRenderTargets(rememberBindings));
        }
    }
}
