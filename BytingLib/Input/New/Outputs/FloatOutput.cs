namespace BytingLib
{
    public class FloatOutput : FloatInput, IPointerValue
    {
        private FloatInput child;

        public FloatOutput(FloatInput child)
        {
            this.child = child;
        }

        public Type GetDeclaredPointerValueType() => typeof(FloatInput);
        public object? GetPointerValue() => child;

        public override float GetValue(FullInput input)
        {
            return child.GetValue(input);
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

        public static implicit operator float(FloatOutput f) => f.Value;
    }
}
