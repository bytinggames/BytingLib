
namespace BytingLib
{
    /// <summary>
    /// Might be bad practice to use this. Probably rather use IntOutputOnChange, because the output is the topmost property in the input tree
    /// </summary>
    public class IntOnChange(IntInput child, int defaultValue = -1) : IntInput
    {
        public override IEnumerable<InputUpdate> GetChildren()
        {
            yield return child;
        }

        protected override int CalculateValue(FullInput input)
        {
            if (child.Delta != 0)
            {
                return child.Value;
            }
            return defaultValue;
        }
    }
}
