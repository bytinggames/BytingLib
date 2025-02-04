namespace BytingLib
{
    [InputShortcut("Or")]
    public class BoolOr(params BoolInput[] Children) : BoolInput
    {
        public BoolInput[] Children { get; } = Children;

        protected override bool CalculateValue(FullInput input)
        {
            for (int i = 0; i < Children.Length; i++)
            {
                if (Children[i].Down)
                {
                    return true;
                }
            }
            return false;
        }


        public override IEnumerable<InputUpdate> GetChildren()
        {
            for (int i = 0; i < Children.Length; i++)
            {
                yield return Children[i];
            }
        }

        public override string ToString()
        {
            return string.Join(" | ", (object?[])Children);
        }
    }
}
