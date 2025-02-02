namespace BytingLib
{
    public class IntOutput : IntInput, IPointerValue, IInputOutput
    {
        protected IntInput child;
        private readonly int disabledValue;

        public IntOutput(IntInput child, int disabledValue = 0)
        {
            this.child = child;
            this.disabledValue = disabledValue;
        }

        public Type GetDeclaredPointerValueType() => typeof(IntInput);
        public object? GetPointerValue() => child;

        protected override int CalculateValue(FullInput input)
        {
            return child.Value;
        }

        public void SetPointerValue(object obj)
        {
            if (obj is IntInput delta)
            {
                child.Dispose();
                child = delta;
                child.Initialize(updater);
            }
        }

        public override IEnumerable<InputUpdate> GetChildren()
        {
            yield return child;
        }

        public override string ToString()
        {
            return "->" + child;
        }

        public void Disable()
        {
            SetPointerValue(new IntConst(disabledValue));
        }

        public static implicit operator int(IntOutput f) => f.Value;
    }
}
