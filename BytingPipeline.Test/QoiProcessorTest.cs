using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate;
using System.Xml;

namespace BytingPipeline.Test
{
    [TestClass]
    public class QoiProcessorTest
    {
        static string imageFile = "Resources\\Image.png";

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