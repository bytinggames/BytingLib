namespace BytingLib
{
    [InputShortcut("Not")]
    public class BoolNot(BoolInput child) : BoolInput
    {
        public override IEnumerable<InputUpdate> GetChildren()
        {
            yield return child;
        }

        public override bool CalculateValue(FullInput input)
        {
            return !child.CalculateValue(input);
        }

        public override string ToString()
        {
            return " NOT " + child;
        }
    }
}
