namespace BytingLib
{
    public class Vector2FromBools(BoolInput up, BoolInput left, BoolInput down, BoolInput right) : Vector2Input
    {
        public override Vector2 GetValue(FullInput input)
        {
            Vector2 v = Vector2.Zero;
            if (left.IsDown(input))
            {
                v.X--;
            }
            if (right.IsDown(input))
            {
                v.X++;
            }
            if (up.IsDown(input))
            {
                v.Y--;
            }
            if (down.IsDown(input))
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
