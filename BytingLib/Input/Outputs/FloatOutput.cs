namespace BytingLib
{
    public class FloatOutput : FloatInput, IPointerValue, IInputOutput
    {
        private FloatInput child;

        public FloatOutput(FloatInput child)
        {
            this.child = child;
        }

        public Type GetDeclaredPointerValueType() => typeof(FloatInput);
        public object? GetPointerValue() => child;

        public override float CalculateValue(FullInput input)
        {
            return child.Value;
        }

        public void SetPointerValue(object obj)
        {
            if (obj is FloatInput delta)
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
            SetPointerValue(new FloatConst(0f));
        }

        public static implicit operator float(FloatOutput f) => f.Value;
    }
}
