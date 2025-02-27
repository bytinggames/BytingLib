using Microsoft.Xna.Framework.Content.Pipeline.Graphics;

namespace BytingPipeline.Test
{
    class TextureTestHelper
    {
        public static void StoreBitmapContentAsPng(BitmapContent bitmapContent, string localFilenameWithoutExtension)
        {
            Directory.CreateDirectory("Output");
            using (FileStream fs = File.Create(Path.Combine("Output", localFilenameWithoutExtension + ".png")))
            {
                StbImageWriteSharp.ImageWriter imageWriter = new();

                imageWriter.WritePng(bitmapContent.GetPixelData(),
                    bitmapContent.Width,
                    bitmapContent.Height,
                    StbImageWriteSharp.ColorComponents.RedGreenBlueAlpha,
                    fs);
            }
        }
    }
}
