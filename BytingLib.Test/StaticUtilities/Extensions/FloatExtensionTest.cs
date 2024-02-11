
namespace BytingLib.Test.StaticUtilities.Extensions
{
    [TestClass]
    public class FloatExtensionTest
    {
        [TestMethod]
        public void TestFloatBinaryIncrement()
        {
            float f = 0;
            f = f.Decrement();
            Assert.IsTrue(f < 0f);
            f = f.GetIncrement();
            Assert.AreEqual(f, 0f);
            f = f.GetIncrement();
            Assert.IsTrue(f > 0f);
            f = f.Decrement();
            Assert.AreEqual(f, 0f);


            for (int i = 0; i < 1000; i++)
            {
                float nextF = f.GetIncrement();
                Assert.AreNotEqual(f, nextF);
                f = nextF;
            }
        }
    }
}
