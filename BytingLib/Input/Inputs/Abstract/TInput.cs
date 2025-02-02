namespace BytingLib
{
    public abstract class TInput<T> : InputUpdate where T : struct
    {
        public T Value { get; protected set; }

        public abstract T GetValue(FullInput input);

        public override void Update(FullInput input)
        {
            Value = GetValue(input);
        }
    }
}
