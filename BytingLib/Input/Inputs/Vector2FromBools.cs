namespace BytingLib
{
    public class Vector2FromBools(InputBool up, InputBool left, InputBool down, InputBool right) : InputVector2Simple
    {
        protected override Vector2 CalculateValue(FullInput input, InputVector2State state)
        {
            Vector2 v = Vector2.Zero;
            if (left.GetState(state.Updater).Down)
            {
                v.X--;
            }
            if (right.GetState(state.Updater).Down)
            {
                v.X++;
            }
            if (up.GetState(state.Updater).Down)
            {
                v.Y--;
            }
            if (down.GetState(state.Updater).Down)
            {
                v.Y++;
            }
            return v;
        }

        public override IEnumerable<Input> GetChildren()
        {
            yield return left;
            yield return right;
            yield return up;
            yield return down;
        }

        public override string ToString()
        {
            return $"{up} {left} {down} {right}";
        }
    }
}
