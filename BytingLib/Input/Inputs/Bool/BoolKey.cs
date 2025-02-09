using Microsoft.Xna.Framework.Input;

namespace BytingLib
{
    public class BoolKey(Keys key) : InputBoolSimple
    {
        public Keys Key { get; } = key;

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
