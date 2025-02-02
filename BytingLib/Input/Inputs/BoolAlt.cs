using Microsoft.Xna.Framework.Input;

namespace BytingLib
{
    [InputShortcut("Alt")]
    public class BoolAlt : BoolInput
    {
        public override IEnumerable<InputUpdate> GetChildren()
        {
            yield break;
        }

        public override bool CalculateValue(FullInput input) => input.KeyState.IsKeyDown(Keys.LeftAlt) || input.KeyState.IsKeyDown(Keys.RightAlt);

        public override string ToString()
        {
            return "Alt";
        }
    }
}
