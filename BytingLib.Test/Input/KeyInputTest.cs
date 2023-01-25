using Microsoft.Xna.Framework.Input;
using System;
using System.Reflection;

namespace BytingLib.Test.Input
{
    [TestClass]
    public class KeyInputTest
    {
        /// <summary>This was used to create and test the code for KeyboardStateDouble_Generated.txt</summary>
        [TestMethod]
        public void Generate_GetKeysFromAssembly()
        {
            string monoGameAssemblyFile = "MonoGame.Framework.dll";
            Assembly a = Assembly.LoadFrom(monoGameAssemblyFile);
            var type = a.GetType("Microsoft.Xna.Framework.Input.Keys");
            if (type == null)
                throw new Exception("type Keys not found");
            var names = type.GetEnumNames();
            string namesJoined = string.Join(' ', names);
            Console.WriteLine(namesJoined);
        }


        [TestMethod]
        public void TestPressDownRelease()
        {
            KeyboardState currentState = default;

            KeyInput keys = new KeyInput(() => currentState);
            Assert.IsFalse(keys.Space.Down);
            Assert.IsFalse(keys.Space.Pressed);
            Assert.IsFalse(keys.Space.Released);

            keys.Update();
            Assert.IsFalse(keys.Space.Down);
            Assert.IsFalse(keys.Space.Pressed);
            Assert.IsFalse(keys.Space.Released);

            Assert.IsFalse(currentState.IsKeyDown(Keys.Space));
            InputSimulation.SimulateSpacePress(ref currentState);

            keys.Update();
            Assert.IsTrue(keys.Space.Down);
            Assert.IsTrue(keys.Space.Pressed);
            Assert.IsFalse(keys.Space.Released);

            keys.Update();
            Assert.IsTrue(keys.Space.Down);
            Assert.IsFalse(keys.Space.Pressed);
            Assert.IsFalse(keys.Space.Released);

            currentState = default; // reset space
            keys.Update();
            Assert.IsFalse(keys.Space.Down);
            Assert.IsFalse(keys.Space.Pressed);
            Assert.IsTrue(keys.Space.Released);

            keys.Update();
            Assert.IsFalse(keys.Space.Down);
            Assert.IsFalse(keys.Space.Pressed);
            Assert.IsFalse(keys.Space.Released);

        }

        [TestMethod]
        public void TestGetKey()
        {
            KeyboardState currentState = default;

            KeyInput keys = new KeyInput(() => currentState);

            Assert.IsFalse(currentState.IsKeyDown(Keys.Space));
            InputSimulation.SimulateSpacePress(ref currentState);

            keys.Update();

            Assert.AreEqual(keys.Space, keys.GetKey(Keys.Space));
            Assert.AreNotEqual(keys.Space, keys.GetKey(Keys.Enter));
        }
    }
}