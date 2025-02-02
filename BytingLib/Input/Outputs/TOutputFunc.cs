namespace BytingLib
{
    public class TOutputFunc<T> : InputUpdate, IInputOutput
    {
        private readonly Func<T> getValue;
        public T Value { get; private set; }

        public TOutputFunc(Func<T> getValue)
        {
            this.getValue = getValue;
            Value = getValue();
        }

        public override IEnumerable<InputUpdate> GetChildren()
        {
            yield break;
        }

        public override void Update(FullInput input)
        {
            Value = getValue();
        }
    }
}
