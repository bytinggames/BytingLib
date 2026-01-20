using BytingLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using System.Threading;

namespace BuildTemplates.Test
{
    [TestClass]
    public class ContentTemplateTest
    {

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void TestBuild(bool loadOnStartup)
        {
            string contentPath = Path.Combine("..", "..", "..", "Content");
            string nameSpace = "BytingLibGame";
            (string output, string mgcbOutput, string locaCode, ShaderFile[] shaders) = ContentTemplate.Create(contentPath, nameSpace, new string[0], loadOnStartup, new ContentConverter());
            Assert.IsNotNull(output);
            Assert.IsNotNull(mgcbOutput);
            Assert.IsNotNull(locaCode);
            Assert.IsNotNull(shaders);
        }

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void TestBuildBytingLibGame(bool loadOnStartup)
        {
            string contentPath = @"C:\Projects\BytingLibGame\BytingLibGame\Content";
            string nameSpace = "BytingLibGame";
            (string output, string mgcbOutput, string locaCode, ShaderFile[] shaders) = ContentTemplate.Create(contentPath, nameSpace, new string[0], loadOnStartup, new ContentConverter());
            Assert.IsNotNull(output);
            Assert.IsNotNull(mgcbOutput);
            Assert.IsNotNull(locaCode);
            Assert.IsNotNull(shaders);
        }

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void TestBuildSE(bool loadOnStartup)
        {
            string contentPath = @"C:\Projects\SE\SE\Content";
            string nameSpace = "SE";
            (string output, string mgcbOutput, string locaCode, ShaderFile[] shaders) = ContentTemplate.Create(contentPath, nameSpace, new string[0], loadOnStartup, new SEContentConverter());
            Assert.IsNotNull(output);
            Assert.IsNotNull(mgcbOutput);
            Assert.IsNotNull(locaCode);
            Assert.IsNotNull(shaders);
        }
    }

    class SEContentConverter : ContentConverter
    {
        public SEContentConverter()
        {
            ProcessorToDataType.Add("CollisionModelProcessor", "CollisionModelBytes");
            ProcessorToDataType.Add("TextureStampProcessor", "Texture2D");
            ProcessorToDataType.Add("TextureShrinkProcessor", "Texture2D");
            ProcessorToDataType.Add("ShortcutProcessor", "ShortcutContent");
            DataTypeToVarExtension.Add("CollisionModelBytes", "Col");
            //RuntimeTypes.Add("CollisionModelBytes", typeof(CollisionModelBytes));
        }
    }

    class Platformer3DContentConverter : ContentConverter
    {
        public Platformer3DContentConverter()
        {
            ProcessorToDataType.Add("MyModelProcessor", "MyModel");

            DataTypeToVarExtension.Add("MyModel", "Model");

            //RuntimeTypes.Add("MyModel", typeof(MyModel));
        }
    }
}