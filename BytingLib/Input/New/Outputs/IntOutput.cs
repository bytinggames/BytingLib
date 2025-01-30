namespace BytingLib
{
    public class IntOutput : IntInput, IPointerValue
    {
        private IntInput child;

        public IntOutput(IntInput child)
        {
            this.child = child;
        }

        public Type GetDeclaredPointerValueType() => typeof(IntInput);
        public object? GetPointerValue() => child;

        public override int GetValue(FullInput input)
        {
            return child.GetValue(input);
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

        public static implicit operator int(IntOutput f) => f.Value;
    }
}
