namespace BytingLib
{
    public class Vector2Mouse : InputVector2Simple
    {
        protected override Vector2 CalculateValue(FullInput input, InputVector2State state)
        {
            return input.MouseState.Position.ToVector2();
        }

        public override string ToString()
        {
            return $"MousePos";
        }

    }
}
