namespace BytingLib
{
    [InputShortcut("Not")]
    public class BoolNot(BoolInput child) : BoolInput
    {
        public override IEnumerable<InputUpdate> GetChildren()
        {
            yield return child;
        }

        protected override bool CalculateValue(FullInput input)
        {
            return !child.Down;
        }

        public override string ToString()
        {
            return " NOT " + child;
        }
    }
}
