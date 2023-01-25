namespace BytingLib.Test.StaticUtilities
{
    [TestClass]
    public class CodeHelperTest
    {
        [TestMethod]
        public void TestSwap()
        {
            Class c1 = new Class() { Variable = 1 };
            Class c2 = new Class() { Variable = 2 };
            CodeHelper.Swap(ref c1, ref c2);
            Assert.AreEqual(2, c1.Variable);
            Assert.AreEqual(1, c2.Variable);

            Struct s1 = new Struct() { Variable = 1 };
            Struct s2 = new Struct() { Variable = 2 };
            CodeHelper.Swap(ref s1, ref s2);
            Assert.AreEqual(2, s1.Variable);
            Assert.AreEqual(1, s2.Variable);
        }

        class Class
        {
            public int Variable { get; set; }
        }

        struct Struct
        {
            public int Variable { get; set; }
        }
    }
}
