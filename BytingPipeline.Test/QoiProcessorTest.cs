using Microsoft.Xna.Framework.Content.Pipeline;

namespace BytingPipeline.Test
{
    [TestClass]
    public class QoiProcessorTest
    {
        static string imageFile = Path.Combine("Resources", "Image.png");

        [TestMethod]
        public void Test()
        {
            var importerContext = new TestImporterContext();
            var contextProcessor = new TestProcessorContext(TargetPlatform.DesktopGL, "OutputImage.xnb");

            TextureImporter importer = new TextureImporter();
            var texContent = importer.Import(imageFile, importerContext);
            QoiProcessor processor = new QoiProcessor();
            var _ = processor.Process(texContent, contextProcessor);
        }
    }
}