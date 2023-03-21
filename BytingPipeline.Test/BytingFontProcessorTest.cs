using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate;
using System.Xml;

namespace BytingPipeline.Test
{
    [TestClass]
    public class BytingFontProcessorTest
    {
        static string ArialFont = @"<?xml version=""1.0"" encoding=""utf-8""?>
<XnaContent xmlns:Graphics=""Microsoft.Xna.Framework.Content.Pipeline.Graphics"">
  <Asset Type=""Graphics:FontDescription"">
    <FontName>Arial</FontName>
    <Size>20</Size>
    <Spacing>0</Spacing>
    <UseKerning>true</UseKerning>
    <Style>Bold</Style>
    <CharacterRegions>
      <CharacterRegion>
        <Start>&#32;</Start>
        <End>&#126;</End>
      </CharacterRegion>
    </CharacterRegions>
  </Asset>
</XnaContent>
";

        [TestMethod]
        public void Test()
        {

            var context = new TestProcessorContext(TargetPlatform.DesktopGL, "Arial.xnb");
            var processor = new BytingFontProcessor()
            {
                TextureFormat =  TextureProcessorOutputFormat.DxtCompressed,
                PremultiplyAlpha = true,
                Thickness = 1,
            };

            FontDescription? fontDescription = null;

            using (var input = XmlReader.Create(new StringReader(ArialFont)))
                fontDescription = IntermediateSerializer.Deserialize<FontDescription>(input, "");
            fontDescription.Identity = new ContentIdentity("Arial.spritefont");

            var output = processor.Process(fontDescription, context);
            Assert.IsNotNull(output, "output should not be null");
            Assert.IsNotNull(output.Texture, "output.Texture should not be null");
            var textureType = output.Texture.Faces[0][0].GetType();

            //FontDescription input;
            //ContentProcessorContext context;
            //BytingFontProcessor processor = new BytingFontProcessor();
            //processor.Process(input, context);
        }
    }
}