using BytingLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
    }

    class SEContentConverter : ContentConverter
    {
        public SEContentConverter()
        {
            ProcessorToDataType.Add("CollisionMeshProcessor", "CollisionMesh");
            ProcessorToDataType.Add("CollisionMeshGridProcessor", "CollisionMeshGrid");

            DataTypeToVarExtension.Add("CollisionMesh", "Col");
            DataTypeToVarExtension.Add("CollisionMeshGrid", "Col");

            //RuntimeTypes.Add("CollisionMesh", typeof(CollisionMesh));
            //RuntimeTypes.Add("CollisionMeshGrid", typeof(CollisionMeshGrid));
        }
    }
}