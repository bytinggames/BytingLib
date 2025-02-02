namespace BytingLib
{
    public class IntMouseWheel : IntInput
    {
        int previousScrollWheelValue;

        public override IEnumerable<InputUpdate> GetChildren()
        {
            yield break;
        }

        public override int GetValue(FullInput input)
        {
            int scroll = input.MouseState.ScrollWheelValue;
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

        public override string ToString()
        {
            return $"MouseWheel ({Value})";
        }
    }
}
