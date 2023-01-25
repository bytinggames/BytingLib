using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace BuildTemplates.Test
{
    [TestClass]
    public class ContentTemplateTest
    {
        // TODO: make independent of global path. include own content here inside the BuildTemplates.Test project (and namespaces below)
        const string contentPath = @"D:\Documents\Visual Studio 2017\Projects\BytingLibGame\BytingLibGame\Content\";

        [TestMethod]
        public void TestBuild()
        {
            (string output, string mgcbOutput, string locaCode) = ContentTemplate.Create(contentPath, "BytingLibGame", new string[0]);
            Assert.IsNotNull(output);
            Assert.IsNotNull(mgcbOutput);
            Assert.IsNotNull(locaCode);
        }
    }
}