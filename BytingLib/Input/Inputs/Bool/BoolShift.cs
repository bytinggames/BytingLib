using Microsoft.Xna.Framework.Input;

namespace BytingLib
{
    public class BoolShift : InputBoolSimple
    {
        protected override bool CalculateValue(FullInput input, InputBoolState state)
        {
            return input.KeyState.IsKeyDown(Keys.LeftShift) || input.KeyState.IsKeyDown(Keys.RightShift);
        }

        public override string ToString()
        {
            return "Shift";
        }
    }
}
