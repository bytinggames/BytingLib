
namespace BytingLib
{
    public class IntOutputOnChange : IntOutput
    {
        private readonly int defaultValue;

        public IntOutputOnChange(IntInput child, int defaultValue = -1) : base(child, defaultValue)
        {
            this.defaultValue = defaultValue;
        }

        public override int CalculateValue(FullInput input)
        {
            if (child.Delta != 0)
            {
                return child.Value;
            }
            return defaultValue;
        }
    }
}
