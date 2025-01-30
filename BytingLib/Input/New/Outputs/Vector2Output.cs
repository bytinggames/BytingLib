namespace BytingLib
{
    public class Vector2Output : Vector2Input, IPointerValue
    {
        private Vector2Input child;

        public Vector2Output(Vector2Input Child)
        {
            child = Child;
        }

        public Type GetDeclaredPointerValueType() => typeof(Vector2Input);
        public object? GetPointerValue() => child;

        public override Vector2 GetValue(FullInput input)
        {
            return child.GetValue(input);
        }

        public void SetPointerValue(object obj)
        {
            if (obj is Vector2Input delta)
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

        public static implicit operator Vector2(Vector2Output v) => v.Value;
    }
}
