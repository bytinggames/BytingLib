namespace BytingLib
{
    public class BoolOutput : BoolInput, IPointerValue
    {
        public BoolInput Value { get; private set; }

        public BoolOutput(BoolInput Value)
        {
            this.Value = Value;
        }

        public object GetPointerValue() => Value;

        public Type GetDeclaredPointerValueType() => typeof(BoolInput);

        public override bool IsDown(FullInput input)
        {
            return Value.IsDown(input);
        }

        public void SetPointerValue(object obj)
        {
            if (obj is BoolInput delta)
            {
                Value.Dispose();
                Value = delta;
                Value.Initialize(updater);
            }
        }

        public override IEnumerable<InputUpdate> GetChildren()
        {
            yield return Value;
        }

        public override string ToString()
        {
            return "->" + Value;
        }
    }
}
