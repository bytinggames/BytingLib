using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BytingLib.Test.Extensions
{
    [TestClass]
    public class AssertExtensionsTest
    {
        [TestMethod]
        public void TestAreEqualItems()
        {
            int[] arr = new int[] { 1, 2, 3 };
            int[] arr2 = new int[] { 1, 2, 3 };

            Assert.That.AreEqualItems(arr, arr2);
            arr2[2] = 0;
            Assert.That.AreNotEqualItems(arr, arr2);
            Assert.That.AreNotEqualItems(arr, new int[] { 1, 2, 3, 4 });
        }
    }
}
