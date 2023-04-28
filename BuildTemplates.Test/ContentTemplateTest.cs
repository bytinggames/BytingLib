using BytingLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework.Graphics;
using System.IO;

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
            string contentPath = @"D:\Documents\Visual Studio 2017\Projects\BytingLibGame\BytingLibGame\Content";
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
            string contentPath = @"D:\Documents\Visual Studio 2017\Projects\SE\SE\Content";
            string nameSpace = "SE";
            (string output, string mgcbOutput, string locaCode, ShaderFile[] shaders) = ContentTemplate.Create(contentPath, nameSpace, new string[0], loadOnStartup, new SEContentConverter());
            Assert.IsNotNull(output);
            Assert.IsNotNull(mgcbOutput);
            Assert.IsNotNull(locaCode);
            Assert.IsNotNull(shaders);
        }

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void TestBuildSELaptop(bool loadOnStartup)
        {
            string contentPath = @"C:\Users\Julian\Documents\Visual Studio 2017\Projects\SE\SE\Content";//@"D:\Documents\Visual Studio 2017\Projects\SE\SE\Content";
            string nameSpace = "SE";
            (string output, string mgcbOutput, string locaCode, ShaderFile[] shaders) = ContentTemplate.Create(contentPath, nameSpace, new string[0], loadOnStartup, new SEContentConverter());
            Assert.IsNotNull(output);
            Assert.IsNotNull(mgcbOutput);
            Assert.IsNotNull(locaCode);
            Assert.IsNotNull(shaders);
        }

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void TestBuildPlatformer3D(bool loadOnStartup)
        {
            string contentPath = @"D:\Documents\Visual Studio 2017\Projects\Platformer3D\Platformer3D\Platformer3D\Content";
            string nameSpace = "Platformer3D";
            (string output, string mgcbOutput, string locaCode, ShaderFile[] shaders) = ContentTemplate.Create(contentPath, nameSpace, new string[0], loadOnStartup, new Platformer3DContentConverter());
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
            ProcessorToDataType.Add("LevelModelProcessor", "LevelModel");

            DataTypeToVarExtension.Add("LevelModel", "Lvl");

            //RuntimeTypes.Add(nameof(LevelModel), typeof(LevelModel));
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