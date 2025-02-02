using Microsoft.Xna.Framework.Input;

namespace BytingLib
{
    [InputShortcut("Key")]
    public class BoolKey(Keys key) : BoolInput
    {
        public override IEnumerable<InputUpdate> GetChildren()
        {
            yield break;
        }

        protected override bool CalculateValue(FullInput input) => input.KeyState.IsKeyDown(key);

        public override string ToString()
        {
            return key.ToString();
        }
    }
}
