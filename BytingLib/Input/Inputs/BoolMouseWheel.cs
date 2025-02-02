namespace BytingLib
{
    public class BoolMouseWheel : BoolInput
    {
        IntMouseWheel mouseWheelSource = new();
        private readonly bool? onlyUpOrDown;

        public BoolMouseWheel(bool? onlyUpOrDown = null)
        {
            this.onlyUpOrDown = onlyUpOrDown;
        }

        public override IEnumerable<InputUpdate> GetChildren()
        {
            yield return mouseWheelSource;
        }

        protected override bool CalculateValue(FullInput input)
        {
            if (mouseWheelSource.Value != 0)
            {
                if (onlyUpOrDown == null)
                {
                    return true;
                }
                else if (onlyUpOrDown.Value)
                {
                    return mouseWheelSource.Value > 0;
                }
                else
                {
                    return mouseWheelSource.Value < 0;
                }
            }
            return false;
        }
    }
}
