namespace BytingLib
{
    public abstract class TInput<T> : InputUpdate where T : struct
    {
        public T Value { get; protected set; }

        public abstract T CalculateValue(FullInput input);

        public override void Update(FullInput input)
        {
            Value = CalculateValue(input);
        }
    }
}
