namespace BytingLib
{
    public class Vector2Flip(InputBool flipX, InputBool flipY) : InputVector2Simple
    {
        public InputBool FlipX { get; } = flipX;
        public InputBool FlipY { get; } = flipY;

        protected override Vector2 CalculateValue(FullInput input, InputVector2State state)
        {
            return new Vector2(
                FlipX.GetState(state.Updater).Down ? -1f : 1f,
                FlipY.GetState(state.Updater).Down ? -1f : 1f
            );
        }

        public override IEnumerable<Input> GetChildren()
        {
            yield return FlipX;
            yield return FlipY;
        }

        public override string ToString()
        {
            return $"{FlipX} {FlipY}";
        }
    }
}
