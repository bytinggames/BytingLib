namespace BytingLib
{
    public class BoolValue(bool value) : InputBoolSimple
    {
        public bool Value { get; set; } = value;

        protected override bool CalculateValue(FullInput input, InputBoolState state)
        {
            return Value;
        }
    }
}

