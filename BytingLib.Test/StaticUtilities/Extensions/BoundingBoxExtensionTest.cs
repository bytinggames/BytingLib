
using System.Collections.Generic;

namespace BytingLib.Test.StaticUtilities.Extensions
{
    [TestClass]
    public class BoundingBoxExtensionTest
    {
        [TestMethod]
        [DynamicData(nameof(GetData), DynamicDataSourceType.Method)]
        public void TestDistanceSquaredToBoundingBox(BoundingBox b2, float distanceSquared)
        {
            BoundingBox b1 = new BoundingBox(Vector3.Zero, new Vector3(1, 2, 3));
            float dist = b1.DistanceSquaredTo(b2);
            Assert.AreEqual(distanceSquared, dist);
        }
        static IEnumerable<object[]> GetData()
        {
            yield return new object[] { new BoundingBox(Vector3.Zero, Vector3.Zero), 0f };
            yield return new object[] { new BoundingBox(new Vector3(-10, -10, -10), new Vector3(0, 0, 0)), 0f };
            yield return new object[] { new BoundingBox(new Vector3(-10, -10, -10), new Vector3(-5, -5, -5)), 5 * 5 * 3 };
            yield return new object[] { new BoundingBox(new Vector3(0, -10, -10), new Vector3(5, -5, -5)), 5 * 5 * 2 };
            yield return new object[] { new BoundingBox(new Vector3(0, 0, -10), new Vector3(5, 5, -5)), 5 * 5 };
            yield return new object[] { new BoundingBox(new Vector3(10, 10, 10), new Vector3(50, 50, 50)), 9 * 9 + 8 * 8 + 7 * 7 };
        }
    }
}
