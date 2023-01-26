using System.Collections.Generic;

namespace BytingLib.Test.Geometry._2D
{
    [TestClass]
    public class CollisionTest
    {
        [TestMethod]
        [DynamicData(nameof(GetData), DynamicDataSourceType.Method)]
        public void TestDistRectangleRectangle(Rect rect1, Rect rect2)
        {
            Vector2[] dirs = new Vector2[]
            {
                new Vector2(1,0),
                new Vector2(0,1),
                new Vector2(-1,0),
                new Vector2(0,-1),
                new Vector2(1.3f, 0.4f),
                new Vector2(-0.3f, 1.6f),
                new Vector2(-2.3f, 3.4f),
                new Vector2(1.3f, -3.4f),
            };
            for (int i = 0; i < 2; i++)
            {

                foreach (var dir in dirs)
                {
                    var crPolygonsCalculation = Collision.DistPolygonPolygon(rect1.ToPolygon(), rect2.ToPolygon(), dir);
                    var crRectCalculation = Collision.DistRectangleRectangle(rect1, rect2, dir);

                    Assert.AreEqual(crPolygonsCalculation.AxisCol, crRectCalculation.AxisCol);
                    Assert.AreEqual(crPolygonsCalculation.AxisColReversed, crRectCalculation.AxisColReversed);
                    Assert.AreEqual(crPolygonsCalculation.Collision, crRectCalculation.Collision);
                    Assert.AreEqual(crPolygonsCalculation.Distance, crRectCalculation.Distance);
                    Assert.AreEqual(crPolygonsCalculation.DistanceReversed, crRectCalculation.DistanceReversed);
                }
                CodeHelper.Swap(ref rect1, ref rect2);
            }
        }

        static IEnumerable<object[]> GetData()
        {
            yield return new object[] { new Rect(0, 0, 10, 10), new Rect(0, 0, 10, 10) };
            yield return new object[] { new Rect(0, 0, 10, 10), new Rect(20, 0, 10, 10) };
            yield return new object[] { new Rect(0, 0, 10, 10), new Rect(20, 20, 10, 10) };
            yield return new object[] { new Rect(0, 0, 10, 10), new Rect(-20, -20, 10, 10) };
            yield return new object[] { new Rect(0, 0, 10, 10), new Rect(0, -20, 10, 10) };
            yield return new object[] { new Rect(0, 0, 10, 10), new Rect(-20, 0, 10, 10) };
            yield return new object[] { new Rect(0, 0, 10, 10), new Rect(7,5,5,8) };
            yield return new object[] { new Rect(0, 0, 10, 10), new Rect(-4,-6,5,8) };
        }
    }
}
