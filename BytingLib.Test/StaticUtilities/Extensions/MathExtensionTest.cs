
namespace BytingLib.Test.StaticUtilities.Extensions
{
    [TestClass]
    public class MathExtensionTest
    {
        [TestMethod]
        public void TestFovConversion()
        {
            float aspectRatio = 1.2f;
            float fovX = 2f;
            float fovY = MathExtension.ToFovY(fovX, aspectRatio);
            float fovXNew = MathExtension.ToFovX(fovY, aspectRatio);
            Assert.AreEqual(fovX, fovXNew, 0.001f);
        }
    }
}
