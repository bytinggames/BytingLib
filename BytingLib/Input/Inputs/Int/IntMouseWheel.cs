using System.Diagnostics;

namespace BytingLib
{
    public class IntMouseWheel : InputInt<IntMouseWheelState>
    {
        Stopwatch sw = new();

        protected override int CalculateValue(FullInput input, IntMouseWheelState state)
        {
            long ms = sw.IsRunning ? sw.ElapsedMilliseconds : 0;

            sw.Restart();
            int diff = state.CalculateValue(input.MouseState.ScrollWheelValue);

            if (ms > 100) // if 100ms passed since last update, we likely experienced no updates here
            {
                return 0;
            }
            return diff;
        }

        public override IntMouseWheelState CreateState(InputUpdater updater)
        {
            return new IntMouseWheelState(updater);
        }

        public override IEnumerable<Input> GetChildren()
        {
            yield break;
        }

        public override string ToString()
        {
            return $"MouseWheel";
        }
    }
}
