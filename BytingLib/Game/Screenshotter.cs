using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace BytingLib
{
    public class Screenshotter : IDisposable
    {
        private Texture2D? screenshotTex;
        private readonly GraphicsDevice gDevice;
        private readonly DefaultPaths paths;
        private readonly SpriteBatch spriteBatch;

        public event Action? OnTakeScreenshot;
        public Int2? Resolution { get; set; } = new Int2(1920, 1080);
        public bool ResolutionAsMax { get; set; } = true;

        public Screenshotter(GraphicsDevice gDevice, DefaultPaths paths, SpriteBatch spriteBatch)
        {
            this.gDevice = gDevice;
            this.paths = paths;
            this.spriteBatch = spriteBatch;
        }

        public string TakeScreenshot(bool randomScreenshot, bool pngOrJpeg = true)
        {
            string path = randomScreenshot ? paths.GetNewRandomScreenshotWithoutEnding() : paths.GetNewScreenshotWithoutEnding();
            path += pngOrJpeg ? ".png" : ".jpg";
            TakeScreenshot(path);
            OnTakeScreenshot?.Invoke();

            return path;
        }

        public void TakeScreenshot(string outputFile)
        {
            CaptureScreenshotAsTexture();

            bool pngOrJpeg = outputFile.EndsWith(".png");

            if (pngOrJpeg)
            {
                screenshotTex.SaveAsPng(outputFile);
            }
            else
            {
                screenshotTex.SaveAsJpeg(outputFile);
            }
        }

        /// <summary>
        /// No need to dispose the texture. It is reused by the <see cref="Screenshotter"/>
        /// </summary>
        [MemberNotNull(nameof(screenshotTex))]
        public Texture2D CaptureScreenshotAsTexture()
        {
            int[] backBuffer = CaptureScreenshotAsIntArrayRgba(out int w, out int h);
            if (screenshotTex == null || screenshotTex.Width != w || screenshotTex.Height != h)
            {
                screenshotTex = new Texture2D(gDevice, w, h, false, gDevice.PresentationParameters.BackBufferFormat);
            }
            screenshotTex.SetData(backBuffer);

            if (Resolution != null)
            {
                screenshotTex = screenshotTex.GetScaleToMaxRes(Resolution.Value, spriteBatch) ?? screenshotTex;
            }

            return screenshotTex;
        }

        public int[] CaptureScreenshotAsIntArrayRgba(out int width, out int height)
        {
            width = gDevice.PresentationParameters.BackBufferWidth;
            height = gDevice.PresentationParameters.BackBufferHeight;
            int[] backBuffer = new int[width * height];
            gDevice.GetBackBufferData(backBuffer);
            return backBuffer;
        }

        public byte[] CaptureScreenshotAsByteArrayRgba(out int width, out int height)
        {
            width = gDevice.PresentationParameters.BackBufferWidth;
            height = gDevice.PresentationParameters.BackBufferHeight;
            byte[] backBuffer = new byte[width * height * 4];
            gDevice.GetBackBufferData(backBuffer);
            return backBuffer;
        }

        /// <summary>Calls <see cref="CaptureScreenshotAsByteArrayRgba"/> and removes the alpha channel manually.</summary>
        public byte[] CaptureScreenshotAsByteArrayRgb(out int width, out int height)
        {
            byte[] source = CaptureScreenshotAsByteArrayRgba(out width, out height);

            byte[] target = new byte[width * height * 3];

            RgbaToRgb(source, target);

            return target;
        }

        public void Dispose()
        {
            screenshotTex?.Dispose();
        }

        static unsafe void RgbaToRgb(byte[] rgbaData, byte[] rgbData)
        {
            if (rgbaData.Length / 4 != rgbData.Length / 3)
            {
                throw new Exception("rgbaData data length must be 4/3 the length of rgbData");
            }
            fixed (byte* rgbPtr = &rgbData[0])
            fixed (byte* rgbLastPtr = &rgbData[rgbData.Length - 1])
            {

                fixed (byte* rgbaPtr = &rgbaData[0])
                {
                    RGB* rgb = (RGB*)rgbPtr;
                    RGBA* rgba = (RGBA*)rgbaPtr;
                    while (rgb <= rgbLastPtr)
                    {
                        *rgb = *(RGB*)rgba;
                        rgb++;
                        rgba++;
                    }
                }
            }
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct RGBA
        {
            public byte r;
            public byte g;
            public byte b;
            public byte a;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RGB
        {
            public byte r;
            public byte g;
            public byte b;
        }
    }
}
