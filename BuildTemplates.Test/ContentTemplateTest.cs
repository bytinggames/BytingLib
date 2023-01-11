using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BuildTemplates.Test
{
    [TestClass]
    public class ContentTemplateTest
    {
        [TestMethod]
        public void TestBuild()
        {
            // TODO: make independent of global path. include own content here inside the BuildTemplates.Test project
            string contentPath = @"D:\Documents\Visual Studio 2017\Projects\FastMonoGameBuild\FastMonoGameBuild\Content\";
            (string output, string mgcbOutput) = ContentTemplate.Create(contentPath);
            Assert.IsNotNull(output);
        }
    }
}