using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework.Input;
using System;
using System.IO;
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
            string userProfileDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string monoGameAssemblyFile = Path.Combine(userProfileDir, @".nuget\packages\monogame.framework.desktopgl\3.8.0.1641\lib\netstandard2.0\MonoGame.Framework.dll");
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

            SimulateSpacePress(ref currentState);

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

            SimulateSpacePress(ref currentState);

            keys.Update();

            Assert.AreEqual(keys.Space, keys.GetKey(Keys.Space));
            Assert.AreNotEqual(keys.Space, keys.GetKey(Keys.Enter));
        }

        private static void SimulateSpacePress(ref KeyboardState currentState)
        {
            // get the field that corresponds to the space key
            var field = currentState.GetType().GetField("_keys1",
                BindingFlags.NonPublic | BindingFlags.Instance);

            Assert.IsFalse(currentState.IsKeyDown(Keys.Space));

            object obj = currentState; // box the struct, so that we can pass the reference to the SetValue() method
            field!.SetValue(obj, (uint)1); // this sets space to pressed
            currentState = (KeyboardState)obj;

            Assert.IsTrue(currentState.IsKeyDown(Keys.Space));
        }
    }
}