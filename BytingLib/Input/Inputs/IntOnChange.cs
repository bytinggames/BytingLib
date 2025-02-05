
namespace BytingLib
{
    /// <summary>
    /// Might be bad practice to use this. Probably rather use IntOutputOnChange, because the output is the topmost property in the input tree
    /// </summary>
    public class IntOnChange(InputInt child, int defaultValue = -1) : InputIntSimple
    {
        protected override int CalculateValue(FullInput input, InputIntState state)
        {
            var childState = child.GetState(state.Updater);
            if (childState.Delta != 0)
            {
                return childState.Value;
            }
            return defaultValue;
        }

        public override IEnumerable<Input> GetChildren()
        {
            yield return child;
        }
    }
}
