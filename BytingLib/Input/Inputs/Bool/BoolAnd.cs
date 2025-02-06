namespace BytingLib
{
    public class BoolAnd : InputBoolSimple
    {
        public InputBool[] Bools { get; }

        public BoolAnd(params InputBool[] bools)
        {
            this.Bools = bools;
        }

        protected override bool CalculateValue(FullInput input, InputBoolState state)
        {
            if (Bools.Length == 0)
            {
                return false;
            }
            long shortestDownTime = long.MaxValue;
            for (int i = 0; i < Bools.Length - 1; i++)
            {
                var s = Bools[i].GetState(state.Updater);
                if (!s.Down)
                {
                    return false;
                }
                shortestDownTime = s.DownTime;
            }

            var lastState = Bools[^1];
            return lastState.GetState(state.Updater).Down && lastState.GetState(state.Updater).DownTime < shortestDownTime;
        }

        public override IEnumerable<Input> GetChildren()
        {
            for (int i = 0; i < Bools.Length; i++)
            {
                yield return Bools[i];
            }
        }

        public override string ToString()
        {
            return string.Join(" + ", (object?[])Bools);
        }
    }
}
