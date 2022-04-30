using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BuildTemplates.Test
{
    [TestClass]
    public class ContentTemplateTest
    {
        [TestMethod]
        public void TestBuild()
        {
            string output = ContentTemplate.Create();
            Assert.IsNotNull(output);
        }
    }
}