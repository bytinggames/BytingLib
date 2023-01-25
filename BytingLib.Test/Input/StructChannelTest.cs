using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace BytingLib.Test.Input
{
    [TestClass]
    public class StructChannelTest
    {
        [TestMethod]
        public void TestMouseState()
        {
            MouseState msInput = new MouseState(0,0,0,ButtonState.Pressed, ButtonState.Released, ButtonState.Pressed, ButtonState.Released, ButtonState.Released);
            
            StructChannel<MouseState> filter = new StructChannel<MouseState>(() => msInput);
            
            var ms = filter.GetState();
            Assert.IsTrue(ms.LeftButton == ButtonState.Pressed);
            Assert.IsTrue(ms.MiddleButton == ButtonState.Released);
            Assert.IsTrue(ms.RightButton == ButtonState.Pressed);
            
            MouseState msCatch = default;
            msCatch.LeftButton = ButtonState.Pressed;
            var catcher = new StructCatcher<MouseState>(msCatch);
            filter.AddListener(catcher);

            ms = filter.GetState();
            Assert.IsTrue(ms.LeftButton == ButtonState.Released);
            Assert.IsTrue(ms.MiddleButton == ButtonState.Released);
            Assert.IsTrue(ms.RightButton == ButtonState.Pressed);

            filter.RemoveListener(catcher);
            ms = filter.GetState();
            Assert.IsTrue(ms.LeftButton == ButtonState.Pressed);
            Assert.IsTrue(ms.MiddleButton == ButtonState.Released);
            Assert.IsTrue(ms.RightButton == ButtonState.Pressed);
        }

        [TestMethod]
        public void TestMouseStateNotify()
        {
            MouseState msInput = new MouseState(0, 0, 0, ButtonState.Pressed, ButtonState.Released, ButtonState.Pressed, ButtonState.Released, ButtonState.Released);
            MouseState msCatch = default;
            msCatch.LeftButton = ButtonState.Pressed;

            int frame = 0;
            int begin = 0;
            int sustain = 0;
            int end = 0;
            int none = 0;


            Queue<MouseState> inputs = new Queue<MouseState>(new List<MouseState>()
            {
                default,
                new MouseState { LeftButton = ButtonState.Pressed },
                new MouseState { LeftButton = ButtonState.Pressed },
                new MouseState { LeftButton = ButtonState.Pressed },
                default,
                default
            });

            StructChannel<MouseState> filter = new StructChannel<MouseState>(inputs.Dequeue);
            var catcher = new StructCatcherNotify<MouseState>(msCatch);
            catcher.OnCatchBegin += () =>
            {
                Assert.AreEqual(1, frame);
                begin++;
            };
            catcher.OnCatchSustain += () =>
            {
                Assert.IsTrue(frame == 2 || frame == 3);
                sustain++;
            };
            catcher.OnCatchEnd += () =>
            {
                Assert.AreEqual(4, frame);
                end++;
            };
            catcher.OnCatchNone += () =>
            {
                Assert.IsTrue(frame == 0 || frame == 5);
                none++;
            };
            filter.AddListener(catcher);

            while (inputs.Count > 0)
            {
                var ms = filter.GetState();
                Assert.IsTrue(ms.LeftButton == ButtonState.Released);
                frame++;
            }

            Assert.AreEqual(1, begin);
            Assert.AreEqual(2, sustain);
            Assert.AreEqual(1, end);
            Assert.AreEqual(2, none);
        }

        [TestMethod]
        public void TestKeyboardState()
        {
            KeyboardState msInput = new KeyboardState(Keys.A, Keys.B);

            StructChannel<KeyboardState> filter = new StructChannel<KeyboardState>(() => msInput);

            var ks = filter.GetState();
            Assert.IsTrue(ks.IsKeyDown(Keys.A));
            Assert.IsTrue(ks.IsKeyDown(Keys.B));

            KeyboardState ksCatch = new KeyboardState(Keys.A);
            var catcher = new StructCatcher<KeyboardState>(ksCatch);
            filter.AddListener(catcher);

            ks = filter.GetState();
            Assert.IsTrue(ks.IsKeyUp(Keys.A));
            Assert.IsTrue(ks.IsKeyDown(Keys.B));

            filter.RemoveListener(catcher);
            ks = filter.GetState();
            Assert.IsTrue(ks.IsKeyDown(Keys.A));
            Assert.IsTrue(ks.IsKeyDown(Keys.B));
        }
    }
}
