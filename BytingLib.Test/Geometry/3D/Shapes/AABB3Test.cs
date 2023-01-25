namespace BytingLib.Test.Geometry._3D.Shapes
{
    [TestClass]
    public class AABB3Test
    {
        [TestMethod]
        public void TestToBox()
        {
            AABB3 aabb = new AABB3(new Vector3(1, 2, 3), new Vector3(4, 5, 6));

            Box3 box = aabb.ToBox();
            Assert.AreEqual(aabb.Center, box.Pos);
            box.Transform.Decompose(out Vector3 scale, out _, out _);
            Assert.AreEqual(aabb.Size, scale);
        }
    }
}
