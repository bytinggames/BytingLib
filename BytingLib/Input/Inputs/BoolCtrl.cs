using Microsoft.Xna.Framework.Input;

namespace BytingLib
{
    public class BoolCtrl : InputBoolSimple
    {
        protected override bool CalculateValue(FullInput input, InputBoolState state)
        {
            return input.KeyState.IsKeyDown(Keys.LeftControl) || input.KeyState.IsKeyDown(Keys.RightControl);
        }

        public override string ToString()
        {
            return "Ctrl";
        }
    }
}
