namespace BytingLib
{
    [InputShortcut("Not")]
    public class BoolNot(BoolInput child) : BoolInput
    {
        public override IEnumerable<InputUpdate> GetChildren()
        {
            yield return child;
        }

        public override bool IsDown(FullInput input)
        {
            return !child.IsDown(input);
        }

        public override string ToString()
        {
            return " NOT " + child;
        }
    }
}
