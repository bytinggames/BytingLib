using BytingLib;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate;
using Microsoft.Xna.Framework.Graphics;
using SkiaSharp;
using System.Drawing;
using System.Xml;

namespace BytingPipeline.Test
{
    [TestClass]
    public class SvgImporterTest
    {
        static string imageFile = "Resources\\Logo.svg";

        [TestMethod]
        public void Test()
        {
            var importerContext = new TestImporterContext();
            var contextProcessor = new TestProcessorContext(TargetPlatform.DesktopGL, "OutputImage.xnb");

            SvgImporter importer = new SvgImporter();
            var svgContent = importer.Import(imageFile, importerContext, out SKBitmap? bmp);
            TextureProcessor processor = new TextureProcessor();
            processor.Process(svgContent, contextProcessor);

            //bmp!...Save("Logo.png");
        }
    }
}