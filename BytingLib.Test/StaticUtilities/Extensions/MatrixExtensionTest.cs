
namespace BytingLib.Test.StaticUtilities.Extensions
{
    [TestClass]
    public class MatrixExtensionTest
    {
        [TestMethod]
        public void TestRotatesBaseAxisToParallelBaseAxis()
        {
            Assert.IsTrue(Matrix.Identity.RotatesBaseAxisToParallelBaseAxis());
            Assert.IsTrue(Matrix.CreateRotationX(MathHelper.PiOver2).RotatesBaseAxisToParallelBaseAxis());
            Assert.IsTrue(Matrix.CreateRotationY(MathHelper.PiOver2).RotatesBaseAxisToParallelBaseAxis());
            Assert.IsTrue(Matrix.CreateRotationZ(MathHelper.PiOver2).RotatesBaseAxisToParallelBaseAxis());
            Assert.IsTrue(
                (Matrix.CreateRotationX(MathHelper.PiOver2)
                * Matrix.CreateRotationY(MathHelper.PiOver2)
                * Matrix.CreateRotationZ(MathHelper.PiOver2))
                    .RotatesBaseAxisToParallelBaseAxis());

            Assert.IsTrue(
                (Matrix.CreateScale(1f,2f,3f)
                * Matrix.CreateRotationX(MathHelper.PiOver2)
                * Matrix.CreateRotationY(MathHelper.PiOver2)
                * Matrix.CreateRotationZ(MathHelper.PiOver2))
                    .RotatesBaseAxisToParallelBaseAxis());

            Assert.IsFalse(Matrix.CreateRotationX(MathHelper.PiOver4).RotatesBaseAxisToParallelBaseAxis());
            Assert.IsFalse(Matrix.CreateRotationY(MathHelper.PiOver4).RotatesBaseAxisToParallelBaseAxis());
            Assert.IsFalse(Matrix.CreateRotationZ(MathHelper.PiOver4).RotatesBaseAxisToParallelBaseAxis());
        }
    }
}
