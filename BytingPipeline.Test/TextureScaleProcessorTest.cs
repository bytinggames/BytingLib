using Microsoft.Xna.Framework.Content.Pipeline;

namespace BytingPipeline.Test
{
    [TestClass]
    public class TextureScaleProcessorTest
    {
        static string imageFile = "Resources\\Discord.png";

        [TestMethod]
        public void Test()
        {
            var importerContext = new TestImporterContext();
            var contextProcessor = new TestProcessorContext(TargetPlatform.DesktopGL, "OutputImage.xnb");

            TextureImporter importer = new TextureImporter();
            var texContent = importer.Import(imageFile, importerContext);
            TextureScaleProcessor processor = new TextureScaleProcessor();
            processor.Width = 32;
            processor.Height = 64;
            texContent = processor.Process(texContent, contextProcessor);
            TextureTestHelper.StoreBitmapContentAsPng(texContent.Faces[0][0], "TextureScaleProcessor");
        }
    }
}
