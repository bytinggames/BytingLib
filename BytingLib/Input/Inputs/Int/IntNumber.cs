using Microsoft.Xna.Framework.Input;

namespace BytingLib
{
    public class IntNumber : InputIntSimple
    {
        InputBool[] numberInputs;

        public IntNumber()
        {
            numberInputs = GetNumberKeys();
        }

        public IntNumber(InputBool[] numberInputs)
        {
            this.numberInputs = numberInputs;
        }

        public override IEnumerable<Input> GetChildren()
        {
            for (int i = 0; i < numberInputs.Length; i++)
            {
                yield return numberInputs[i];
            }
        }

        protected override int CalculateValue(FullInput input, InputIntState state)
        {
            for (int i = 0; i < numberInputs.Length; i++)
            {
                if (numberInputs[i].GetState(state.Updater).Down)
                {
                    return i;
                }
            }
            return -1;
        }

        public static InputBool[] GetNumberKeys()
        {
            return [
                new BoolKey(Keys.D0),
                new BoolKey(Keys.D1),
                new BoolKey(Keys.D2),
                new BoolKey(Keys.D3),
                new BoolKey(Keys.D4),
                new BoolKey(Keys.D5),
                new BoolKey(Keys.D6),
                new BoolKey(Keys.D7),
                new BoolKey(Keys.D8),
                new BoolKey(Keys.D9),
            ];
        }
    }
}
