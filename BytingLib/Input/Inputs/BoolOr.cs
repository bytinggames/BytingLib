namespace BytingLib
{
    [InputShortcut("Or")]
    public class BoolOr(params BoolInput[] bools) : BoolInput
    {
        protected override bool CalculateValue(FullInput input)
        {
            for (int i = 0; i < bools.Length; i++)
            {
                if (bools[i].Down)
                {
                    return true;
                }
            }
            return false;
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
            return string.Join(" | ", (object?[])bools);
        }
    }
}
