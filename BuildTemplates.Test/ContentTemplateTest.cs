using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace BuildTemplates.Test
{
    [TestClass]
    public class ContentTemplateTest
    {
        //readonly string contentPath = Path.Combine("..", "..", "..", "Content");
        readonly string contentPath = @"D:\Documents\Visual Studio 2017\Projects\SE\SE\Content";
        readonly string nameSpace = "BytingLibGame";

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void TestBuild(bool loadOnStartup)
        {
            (string output, string mgcbOutput, string locaCode, ShaderFile[] shaders) = ContentTemplate.Create(contentPath, nameSpace, new string[0], loadOnStartup);
            Assert.IsNotNull(output);
            Assert.IsNotNull(mgcbOutput);
            Assert.IsNotNull(locaCode);
            Assert.IsNotNull(shaders);
        }
    }
}