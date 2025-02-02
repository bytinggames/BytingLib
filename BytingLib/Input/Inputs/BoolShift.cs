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

        protected override bool CalculateValue(FullInput input) => input.KeyState.IsKeyDown(Keys.LeftShift) || input.KeyState.IsKeyDown(Keys.RightShift);

        public override string ToString()
        {
            return "Shift";
        }
    }
}
