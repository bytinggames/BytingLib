using Microsoft.Xna.Framework.Input;

namespace BytingLib
{
    public class BoolAlt : InputBoolSimple
    {
        protected override bool CalculateValue(FullInput input, InputBoolState state)
        {
            return input.KeyState.IsKeyDown(Keys.LeftAlt) || input.KeyState.IsKeyDown(Keys.RightAlt);
        }

        public override string ToString()
        {
            return "Alt";
        }
    }
}
