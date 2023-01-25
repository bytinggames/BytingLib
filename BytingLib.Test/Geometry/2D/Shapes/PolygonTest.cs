using System.Collections.Generic;

namespace BytingLib.Test.Geometry._2D.Shapes
{
    [TestClass]
    public class PolygonTest
    {
        [TestMethod]
        public void TestClosed()
        {
            Polygon p = new Polygon(Vector2.Zero, new List<Vector2>(), Polygon.ClosedType.Closed);
            Assert.IsTrue(p.LastEdgeClosed);
            Assert.IsTrue(p.StartCorner);
            Assert.IsTrue(p.EndCorner);
            p.Closed = Polygon.ClosedType.OpenWithStartAndEndCorners;
            Assert.IsFalse(p.LastEdgeClosed);
            Assert.IsTrue(p.StartCorner);
            Assert.IsTrue(p.EndCorner);
            p.Closed = Polygon.ClosedType.OpenWithStartCorner;
            Assert.IsFalse(p.LastEdgeClosed);
            Assert.IsTrue(p.StartCorner);
            Assert.IsFalse(p.EndCorner);
            p.Closed = Polygon.ClosedType.OpenWithEndCorner;
            Assert.IsFalse(p.LastEdgeClosed);
            Assert.IsFalse(p.StartCorner);
            Assert.IsTrue(p.EndCorner);
            p.Closed = Polygon.ClosedType.OpenWithoutCorners;
            Assert.IsFalse(p.LastEdgeClosed);
            Assert.IsFalse(p.StartCorner);
            Assert.IsFalse(p.EndCorner);
        }
    }
}
