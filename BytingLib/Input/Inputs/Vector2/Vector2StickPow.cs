
namespace BytingLib
{
    public class Vector2StickPow(float curveExponent, InputVector2 child) : InputVector2Simple
    {
        public float CurveExponent { get; set; } = curveExponent;
        public InputVector2 Child { get; } = child;

        protected override Vector2 CalculateValue(FullInput fullInput, InputVector2State state)
        {
            Vector2 input = Child.GetState(state.Updater).Value;
            if (input != Vector2.Zero)
            {
                float moveLength = input.Length();
                // make diagonal input not faster than horizontal/vertical
                if (moveLength > 1f)
                {
                    moveLength = 1f;
                }
                moveLength = MathF.Pow(moveLength, CurveExponent);
                input = Vector2.Normalize(input) * moveLength;
            }
            return input;
        }

        public override IEnumerable<Input> GetChildren()
        {
            yield return Child;
        }
    }
}
