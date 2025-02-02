using Microsoft.Xna.Framework.Input;

namespace BytingLib
{
    [InputShortcut("GamePad")]
    public class BoolGamePad(Buttons button) : BoolInput
    {
        public override bool CalculateValue(FullInput input)
        {
            return input.GamePadState.IsButtonDown(button);
        }
        public override IEnumerable<InputUpdate> GetChildren() { yield break; }

        public override string ToString()
        {
            return "GamePad." + button.ToString();
        }
    }
}
