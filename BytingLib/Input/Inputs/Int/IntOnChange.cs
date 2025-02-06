
namespace BytingLib
{
    /// <summary>
    /// Might be bad practice to use this. Probably rather use IntOutputOnChange, because the output is the topmost property in the input tree
    /// </summary>
    public class IntOnChange(InputInt child, int defaultValue = -1) : InputIntSimple
    {
        public InputInt Child { get; } = child;
        public int DefaultValue { get; } = defaultValue;

        protected override int CalculateValue(FullInput input, InputIntState state)
        {
            var childState = Child.GetState(state.Updater);
            if (childState.Delta != 0)
            {
                return childState.Value;
            }
            return DefaultValue;
        }

        public override IEnumerable<Input> GetChildren()
        {
            yield return Child;
        }
    }
}
