using Microsoft.Xna.Framework.Input;

namespace BytingLib
{
    [InputShortcut("Shift")]
    public class BoolShift : BoolInput
    {
        public override IEnumerable<InputUpdate> GetChildren()
        {
            yield break;
        }

        public override bool IsDown(FullInput input) => input.KeyState.IsKeyDown(Keys.LeftShift) || input.KeyState.IsKeyDown(Keys.RightShift);

        public override string ToString()
        {
            return "Shift";
        }
    }
}
