namespace BytingLib
{
    public class IntMouseWheelState : InputIntState
    {
        int previousScrollWheelValue;

        public IntMouseWheelState(InputUpdater updater)
            : base(updater)
        {
        }

        public int CalculateValue(int scroll)
        {
            long scrollDiff = (long)scroll - previousScrollWheelValue;
            if (Math.Abs(scrollDiff) >= int.MaxValue) // check if value overflew (int.MaxValue is only half the range, but still suffices, because you can't scroll that much in a single update)
            {
                // correct overflow
                const long intRange = (long)int.MaxValue * 2 + 1;
                scrollDiff = Math.Sign(-scrollDiff) * (intRange - Math.Abs(scrollDiff));
            }

            previousScrollWheelValue = scroll;
            return (int)scrollDiff;
        }
    }
}
