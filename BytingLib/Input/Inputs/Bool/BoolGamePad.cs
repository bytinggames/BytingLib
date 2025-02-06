using Microsoft.Xna.Framework.Input;

namespace BytingLib
{
    public class BoolGamePad(Buttons button) : InputBoolSimple
    {
        public Buttons Button { get; } = button;

        protected override bool CalculateValue(FullInput input, InputBoolState state)
        {
            return input.GamePadState.IsButtonDown(Button);
        }

        public override string ToString()
        {
            return Button.ToString();
        }
    }
}
