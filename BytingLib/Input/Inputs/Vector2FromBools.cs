namespace BytingLib
{
    public class Vector2FromBools(BoolInput up, BoolInput left, BoolInput down, BoolInput right) : Vector2Input
    {
        protected override Vector2 CalculateValue(FullInput input)
        {
            Vector2 v = Vector2.Zero;
            if (left.Down)
            {
                v.X--;
            }
            if (right.Down)
            {
                v.X++;
            }
            if (up.Down)
            {
                v.Y--;
            }
            if (down.Down)
            {
                v.Y++;
            }
            return v;
        }

        public override IEnumerable<InputUpdate> GetChildren()
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
