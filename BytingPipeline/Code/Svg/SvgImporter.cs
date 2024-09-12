using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using System.IO;
using System;
using Svg;
using Svg.Skia;
using SkiaSharp;
using System.Text;
using System.Runtime.InteropServices;
using System.Drawing;

namespace BytingPipeline
{
    [ContentImporter(".svg", DisplayName = "SvgImporter", DefaultProcessor = "TextureProcessor")]
    public class SvgImporter : ContentImporter<TextureContent>
    {
        static SvgImporter()
        {
            //// load all fonts from Content/Fonts/Svg directory
            //string svgFontDir = Path.Combine(Environment.CurrentDirectory, "Fonts", "Svg");
            //if (Directory.Exists(svgFontDir))
            //{
            //    string[] fonts = Directory.GetFiles(svgFontDir, "*.ttf");
            //    foreach (var font in fonts)
            //    {
            //        SvgFontManager.PrivateFontPathList.Add(font);
            //    }
            //}
        }

        public override TextureContent Import(string filename, ContentImporterContext context)
        {
            return Import(filename, context, out _);
        }

        public TextureContent Import(string filename, ContentImporterContext context, out SKBitmap? bmp)
        {
            bmp = null;
            var output = new Texture2DContent { Identity = new ContentIdentity(filename) };

            BitmapContent face;
            using (var mySvg = new SKSvg())
            {
                mySvg.Load(filename);

                if (mySvg.Picture != null)
                {
                    int width = (int)MathF.Ceiling(mySvg.Picture.CullRect.Width);
                    int height = (int)MathF.Ceiling(mySvg.Picture.CullRect.Height);

                    bmp = new(width, height);
                    SKCanvas canvas = new SKCanvas(bmp);
                    canvas.DrawPicture(mySvg.Picture);

                    var length = width * height * 4;

                    byte[] bytesConverted = new byte[length];

                    face = new PixelBitmapContent<Microsoft.Xna.Framework.Color>(width, height);

                    //int j = 0;
                    //for (int i = 0; i < bmp.Pixels.Length; i++, j += 4)
                    //{
                    //    var p = bmp.Pixels[i];
                    //    bytesConverted[j] = p.Red;
                    //    bytesConverted[j + 1] = p.Green;
                    //    bytesConverted[j + 2] = p.Blue;
                    //    bytesConverted[j + 3] = p.Alpha;
                    //}

                    face.SetPixelData(bytesConverted);
                    output.Faces[0].Add(face);
                }
            }
            return output;
        }
    }
}