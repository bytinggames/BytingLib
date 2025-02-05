using Microsoft.Xna.Framework.Input;
using System.Text.Json.Serialization;

namespace BytingLib
{
    public class BoolKey(Keys Key) : InputBoolSimple
    {
        public Keys Key { get; } = Key;

        protected override bool CalculateValue(FullInput input, InputBoolState state)
        {
            return input.KeyState.IsKeyDown(Key);
        }

        public override string ToString()
        {
            return Key.ToString();
        }
    }
}
