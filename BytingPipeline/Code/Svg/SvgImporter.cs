using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using System.IO;
using System;
using Microsoft.Xna.Framework;
using Svg;
using System.Text;
using System.Runtime.InteropServices;
using System.Drawing;

namespace BytingPipeline
{
    [ContentImporter(".svg", DisplayName = "SvgImporter", DefaultProcessor = "TextureProcessor")]
    public class SvgImporter : ContentImporter<TextureContent>
    {
        public override TextureContent Import(string filename, ContentImporterContext context)
        {
            return Import(filename, context, out _);
        }

        public TextureContent Import(string filename, ContentImporterContext context, out Bitmap bmp)
        {
            var output = new Texture2DContent { Identity = new ContentIdentity(filename) };

            BitmapContent face;
            string svgString = File.ReadAllText(filename, Encoding.UTF8);
            var mySvg = SvgDocument.FromSvg<SvgDocument>(svgString);

            var width = (int)MathF.Ceiling(mySvg.Width.Value);
            var height = (int)MathF.Ceiling(mySvg.Height.Value);

            bmp = mySvg.Draw(width, height);

            var bitmapData = bmp.LockBits(new System.Drawing.Rectangle(0,0,width, height), 
                System.Drawing.Imaging.ImageLockMode.ReadOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            var length = bitmapData.Stride * bitmapData.Height;

            byte[] bytes = new byte[length];
            byte[] bytesConverted = new byte[length];

            // Copy bitmap to byte[]
            Marshal.Copy(bitmapData.Scan0, bytes, 0, length);
            bmp.UnlockBits(bitmapData);


            Console.WriteLine(bytes.Length);
            Console.WriteLine("bytes.Length");

            face = new PixelBitmapContent<Microsoft.Xna.Framework.Color>(width, height);

            for (int i = 0; i < bytes.Length; i += 4)
            {
                bytesConverted[i] = bytes[i + 2];
                bytesConverted[i + 1] = bytes[i + 1];
                bytesConverted[i + 2] = bytes[i];
                bytesConverted[i + 3] = bytes[i + 3];
            }

            face.SetPixelData(bytesConverted);
            output.Faces[0].Add(face);
            return output;
        }
    }
}