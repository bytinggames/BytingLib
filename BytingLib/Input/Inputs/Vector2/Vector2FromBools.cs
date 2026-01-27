namespace BytingLib
{
    public class Vector2FromBools(InputBool up, InputBool left, InputBool down, InputBool right) : InputVector2Simple
    {
        public InputBool Up { get; } = up;
        public InputBool Left { get; } = left;
        public InputBool Down { get; } = down;
        public InputBool Right { get; } = right;

        protected override Vector2 CalculateValue(FullInput input, InputVector2State state)
        {
            Vector2 v = Vector2.Zero;
            if (Left.GetState(state.Updater).Down)
            {
                v.X--;
            }
            if (Right.GetState(state.Updater).Down)
            {
                v.X++;
            }
            if (Up.GetState(state.Updater).Down)
            {
                v.Y--;
            }
            if (Down.GetState(state.Updater).Down)
            {
                v.Y++;
            }
            return v;
        }

        public override IEnumerable<Input> GetChildren()
        {
            yield return Up;
            yield return Left;
            yield return Down;
            yield return Right;
        }

        public override string ToString()
        {
            return $"{Up} {Left} {Down} {Right}";
        }
    }
}
