namespace BytingLib
{
    public class BoolAnd(params InputBool[] bools) : InputBoolSimple
    {
        protected override bool CalculateValue(FullInput input, InputBoolState state)
        {
            if (bools.Length == 0)
            {
                return false;
            }
            long shortestDownTime = long.MaxValue;
            for (int i = 0; i < bools.Length - 1; i++)
            {
                var s = bools[i].GetState(state.Updater);
                if (!s.Down)
                {
                    return false;
                }
                shortestDownTime = s.DownTime;
            }

            var lastState = bools[^1];
            return lastState.GetState(state.Updater).Down && lastState.GetState(state.Updater).DownTime < shortestDownTime;
        }

        public override IEnumerable<Input> GetChildren()
        {
            for (int i = 0; i < bools.Length; i++)
            {
                yield return bools[i];
            }
        }

        public override string ToString()
        {
            return string.Join(" + ", (object?[])bools);
        }
    }
}
