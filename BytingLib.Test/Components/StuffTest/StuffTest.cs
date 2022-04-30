using BytingLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace BytingLib.Test.Components.StuffTest
{
    [TestClass]
    public partial class StuffTest
    {
        [TestMethod]
        public void FailAddNull()
        {
            Stuff stuff = new Stuff(typeof(IDisposable));
            bool catched = false;
            try
            {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                stuff.Add(null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
            }
            catch (ArgumentException)
            {
                catched = true;
            }
            Assert.IsTrue(catched);
        }

        [TestMethod]
        public void TwoInterfaces()
        {
            Stuff stuff = new Stuff(typeof(IGetName), typeof(IGetID));

            stuff.Add(new Name("Harald"));
            stuff.Add(new ID(23));
            stuff.Add(new NameID(7, "Petra"));
            stuff.Add(new ID(345));
            stuff.Add(new NameID(6, "Ingrid"));
            stuff.Add(new Name("Hans"));

            Assert.AreEqual("Harald Petra Ingrid Hans",
                string.Join(" ", stuff.Get<IGetName>().Select(f => f.GetName())));
            Assert.AreEqual("23 7 345 6",
                string.Join(" ", stuff.Get<IGetID>().Select(f => f.GetID())));
        }
    }
}
