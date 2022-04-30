using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BytingLib.Test.Components
{
    [TestClass]
    public class StuffDisposableTest
    {
        [TestMethod]
        public void TestDisposal()
        {
            Disposable[] disposables = new Disposable[]
            {
                new(), new(), new()
            };

            using (StuffDisposable stuff = new StuffDisposable())
            {
                for (int i = 0; i < disposables.Length; i++)
                    stuff.Add(disposables[i]);
            }
            Assert.IsTrue(disposables.All(f => f.Disposed));
        }

        class Disposable : IDisposable
        {
            public bool Disposed { get; set; } = false;
            public void Dispose()
            {
                Disposed = true;
            }
        }
    }
}
