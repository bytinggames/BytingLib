using Microsoft.Xna.Framework.Input;
using System.Reflection;

namespace BytingLib.Test.Input
{
    internal class InputSimulation
    {
        internal static void SimulateSpacePress(ref KeyboardState currentState)
        {
            SimulatePress(ref currentState, 1, 1);
            Assert.IsTrue(currentState.IsKeyDown(Keys.Space));
        }

        internal static void SimulatePress(ref KeyboardState currentState, int keysFieldIndex, uint value)
        {
            // get the field that corresponds to the space key
            var field = currentState.GetType().GetField("_keys" + keysFieldIndex,
                BindingFlags.NonPublic | BindingFlags.Instance);


            object obj = currentState; // box the struct, so that we can pass the reference to the SetValue() method
            field!.SetValue(obj, (uint)value); // this sets space to pressed
            currentState = (KeyboardState)obj;
        }
    }
}
