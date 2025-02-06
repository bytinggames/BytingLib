namespace BytingLib
{
    public class BoolOr(params InputBool[] children) : InputBoolSimple
    {
        public InputBool[] Children { get; } = children;

        protected override bool CalculateValue(FullInput input, InputBoolState state)
        {
            for (int i = 0; i < Children.Length; i++)
            {
                if (Children[i].GetState(state.Updater).Down)
                {
                    return true;
                }
            }
            return false;
        }
        public override IEnumerable<Input> GetChildren()
        {
            return Children;
        }

        public override string ToString()
        {
            return string.Join(" | ", (object?[])Children);
        }

    }
}
