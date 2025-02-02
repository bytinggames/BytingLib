namespace BytingLib
{
    public class TOutputFunc<T> : InputUpdate, IInputOutput where T : struct
    {
        private Func<T> getValue;
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

        public void SetPointerValue(object obj)
        {
            if (obj is Func<T> func)
            {
                getValue = func;
            }
        }

        public object? GetPointerValue()
        {
            return getValue;
        }

        public Type GetDeclaredPointerValueType()
        {
            return getValue.GetType();
        }

        public void Disable()
        {
            getValue = () => default;
        }
    }
}
