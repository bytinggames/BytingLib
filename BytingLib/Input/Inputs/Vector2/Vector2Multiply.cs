namespace BytingLib
{
    public class Vector2Multiply(float factor, InputVector2 child) : InputVector2Simple
    {
        public float Factor { get; } = factor;
        public InputVector2 Child { get; } = child;

        protected override Vector2 CalculateValue(FullInput input, InputVector2State state)
        {
            return Factor * Child.GetState(state.Updater).Value;
        }

        public override IEnumerable<Input> GetChildren()
        {
            yield return Child;
        }

        public override string ToString()
        {
            return $"{Factor} x {Child})";
        }
    }
}
