using System;
using System.Linq;

namespace BytingLib.Test.Components.StuffTest
{
    [TestClass]
    public partial class StuffTest
    {
        [TestMethod]
        public void TestFailAddNull()
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
        public void TestTwoInterfaces()
        {
            Stuff stuff = new Stuff(typeof(IName), typeof(IID));

            stuff.Add(new Name("Harald"));
            stuff.Add(new ID(23));
            stuff.Add(new NameID(7, "Petra"));
            stuff.Add(new ID(345));
            stuff.Add(new NameID(6, "Ingrid"));
            stuff.Add(new Name("Hans"));

            Assert.AreEqual("Harald Petra Ingrid Hans",
                string.Join(" ", stuff.Get<IName>().Select(f => f.GetName())));
            Assert.AreEqual("23 7 345 6",
                string.Join(" ", stuff.Get<IID>().Select(f => f.GetID())));
        }

        [TestMethod]
        public void TestRemoveEach()
        {
            Stuff stuff = new Stuff(typeof(IName), typeof(IID));

            stuff.Add(new Name("Harald"));
            stuff.Add(new ID(23));
            stuff.Add(new NameID(7, "Petra"));
            stuff.Add(new ID(345));
            stuff.Add(new NameID(6, "Ingrid"));
            stuff.Add(new Name("Hans"));

            Assert.AreEqual(4, stuff.Get<IName>().Count);
            stuff.ForEach<IName>(f => stuff.Remove(f));
            Assert.AreEqual(0, stuff.Get<IName>().Count);

            Assert.AreEqual(2, stuff.Get<IID>().Count);
            stuff.ForEach<IID>(f => stuff.Remove(f));
            Assert.AreEqual(0, stuff.Get<IID>().Count);
        }

        [TestMethod]
        public void TestAddWhileInForEach()
        {
            Stuff stuff = new Stuff(typeof(IName));

            stuff.Add(new Name("Hans"));
            stuff.ForEach<IName>(f =>
            {
                int count = stuff.Get<IName>().Count;
                if (count < 10)
                    stuff.Add(new Name("Hans " + count));
            });

            Assert.AreEqual(10, stuff.Get<IName>().Count);
        }

        [TestMethod]
        public void TestGetAndForEach()
        {
            Stuff stuff = new Stuff(typeof(IName), typeof(IID));

            stuff.Add(new Name("Harald"));
            stuff.Add(new ID(23));
            stuff.Add(new NameID(7, "Petra"));
            stuff.Add(new ID(345));
            stuff.Add(new NameID(6, "Ingrid"));
            stuff.Add(new Name("Hans"));

            int count = 0;
            stuff.ForEach<IName>(f => count++);
            Assert.AreEqual(count, stuff.Get<IName>().Count);

            count = 0;
            stuff.ForEach<IID>(f => count++);
            Assert.AreEqual(count, stuff.Get<IID>().Count);
        }

        [TestMethod]
        public void TestAddAndRemoveDuplicate()
        {
            Stuff stuff = new Stuff(typeof(IName));
            var n = new Name("Harald");
            stuff.Add(n);
            stuff.Add(n);
            Assert.AreEqual(2, stuff.Get<IName>().Count);
            stuff.Remove(n);
            Assert.AreEqual(1, stuff.Get<IName>().Count);
            stuff.Remove(n);
            Assert.AreEqual(0, stuff.Get<IName>().Count);
            stuff.Remove(n);
            Assert.AreEqual(0, stuff.Get<IName>().Count);
        }

        [TestMethod]
        public void TestMissingInterface()
        {
            Stuff stuff = new Stuff(typeof(IName));
            Assert.ThrowsException<ArgumentException>(() => stuff.Add(new ID(4325)));
        }

        [TestMethod]
        public void TestNoGivenInterface()
        {
            Assert.ThrowsException<ArgumentException>(() => new Stuff());
        }

        [TestMethod]
        public void TestNoInterfaceType()
        {
            Assert.ThrowsException<ArgumentException>(() => new Stuff(typeof(object)));
            Assert.ThrowsException<ArgumentException>(() => new Stuff(typeof(int)));
            Assert.ThrowsException<ArgumentException>(() => new Stuff(typeof(Exception)));
        }

        [TestMethod]
        public void TestGetWrongType()
        {
            Stuff stuff = new Stuff(typeof(IName));
            Assert.ThrowsException<ArgumentException>(() => stuff.Get<IID>());
        }
    }
}
