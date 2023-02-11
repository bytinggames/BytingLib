namespace BytingLib
{
    class Screenshotter : IDisposable
    {
        private Texture2D? screenshotTex;
        private readonly GraphicsDevice gDevice;
        private readonly DefaultPaths paths;

        public Screenshotter(GraphicsDevice gDevice, DefaultPaths paths)
        {
            this.gDevice = gDevice;
            this.paths = paths;
        }

        public void TakeScreenshot()
        {
            int w = gDevice.PresentationParameters.BackBufferWidth;
            int h = gDevice.PresentationParameters.BackBufferHeight;
            int[] backBuffer = new int[w * h];
            gDevice.GetBackBufferData(backBuffer);
            if (screenshotTex == null || screenshotTex.Width != w || screenshotTex.Height != h)
                screenshotTex = new Texture2D(gDevice, w, h, false, gDevice.PresentationParameters.BackBufferFormat);
            screenshotTex.SetData(backBuffer);
            screenshotTex.SaveAsPng(paths.GetNewScreenshotPng());
        }

        public void Dispose()
        {
            screenshotTex?.Dispose();
        }
    }
}
