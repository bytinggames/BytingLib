namespace BytingLib
{
    [InputShortcut("And")]
    public class BoolAnd(params BoolInput[] bools) : BoolInput
    {
        public override bool CalculateValue(FullInput input)
        {
            if (bools.Length == 0)
            {
                return false;
            }
            long shortestDownTime = long.MaxValue;
            for (int i = 0; i < bools.Length - 1; i++)
            {
                if (!bools[i].Down)
                {
                    return false;
                }
                shortestDownTime = bools[i].DownTime;
            }

            var lastState = bools[^1];
            return lastState.Down && lastState.DownTime < shortestDownTime;
        }

        public override IEnumerable<InputUpdate> GetChildren()
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
