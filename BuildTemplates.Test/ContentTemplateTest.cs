using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace BuildTemplates.Test
{
    [TestClass]
    public class ContentTemplateTest
    {
        readonly string contentPath = Path.Combine("..", "..", "..", "Content");

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void TestBuild(bool loadOnStartup)
        {
            (string output, string mgcbOutput, string locaCode) = ContentTemplate.Create(contentPath, "BytingLibGame", new string[0], loadOnStartup);
            Assert.IsNotNull(output);
            Assert.IsNotNull(mgcbOutput);
            Assert.IsNotNull(locaCode);
        }
    }
}