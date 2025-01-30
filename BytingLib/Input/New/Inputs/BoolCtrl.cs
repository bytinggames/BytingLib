using Microsoft.Xna.Framework.Input;

namespace BytingLib
{
    [InputShortcut("Ctrl")]
    public class BoolCtrl : BoolInput
    {
        public override IEnumerable<InputUpdate> GetChildren()
        {
            yield break;
        }

        public override bool IsDown(FullInput input) => input.KeyState.IsKeyDown(Keys.LeftControl) || input.KeyState.IsKeyDown(Keys.RightControl);

        public override string ToString()
        {
            return "Ctrl";
        }
    }
}
