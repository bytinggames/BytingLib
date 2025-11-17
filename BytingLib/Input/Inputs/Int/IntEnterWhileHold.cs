
namespace BytingLib
{
    public class IntEnterWhileHold(InputBool hold, int defaultValue = -1, int? enterAtDigitCount = null) : InputInt<IntNumberWhileHoldState>
    {
        public InputBool Child { get; } = hold;
        public int DefaultValue { get; } = defaultValue;
        public int? EnterAtDigitCount { get; } = enterAtDigitCount;
        private IntOnChange numberInput = new IntOnChange(new IntNumber(), -1);

        protected override int CalculateValue(FullInput input, IntNumberWhileHoldState state)
        {
            var boolState = Child.GetState(state.Updater);

            if (boolState.Down)
            {
                var numberState = numberInput.GetState(state.Updater);
                if (numberState.Value != -1)
                {
                    int number = state.AccumulatedNumber;
                    if (number == -1)
                    {
                        number = 0;
                    }
                    number *= 10;
                    number += numberState.Value;
                    if (number < 0)
                    {
                        number = int.MaxValue;
                    }
                    state.AccumulatedNumber = number;
                }
            }

            if (state.AccumulatedNumber != -1)
            {
                if (!boolState.Down
                    || EnterAtDigitCount != null && Math.Log10(state.AccumulatedNumber) + 1 >= EnterAtDigitCount.Value)
                {
                    var number = state.AccumulatedNumber;
                    state.AccumulatedNumber = -1;
                    return number;
                }
            }
            return DefaultValue;
        }

        public override IEnumerable<Input> GetChildren()
        {
            yield return numberInput;
            yield return Child;
        }

        public override IntNumberWhileHoldState CreateState(InputUpdater updater)
        {
            return new IntNumberWhileHoldState(updater);
        }
    }

    public class IntNumberWhileHoldState : InputIntState
    {
        public int AccumulatedNumber { get; set; } = -1;

        public IntNumberWhileHoldState(InputUpdater updater) : base(updater)
        {
        }
    }
}
