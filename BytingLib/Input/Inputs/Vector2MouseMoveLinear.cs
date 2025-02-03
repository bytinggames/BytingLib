namespace BytingLib
{
    /// <summary>Without mouse acceleration</summary>
    public class Vector2MouseMoveLinear : Vector2Input
    {
        private readonly Vector2Input mousePos = new Vector2Mouse();
        private readonly BoolInput windowActive = new BoolWindowActive();
        private readonly IPreventMouseMovement preventMouseMovement;

        private int ignoreMouseMovementForNextUpdates = 2; // if the player moves the mouse while a level is loading, it shouldn't affect the view direction

        public Vector2MouseMoveLinear(IPreventMouseMovement preventMouseMovement)
        {
            this.preventMouseMovement = preventMouseMovement;
        }

        protected override Vector2 CalculateValue(FullInput input)
        {
            if (windowActive.Pressed || preventMouseMovement.PreventMouseMovement)
            {
                PreventMouseMovement();
            }

            if (ignoreMouseMovementForNextUpdates-- > 0)
            {
                return Vector2.Zero;
            }
            else
            {
                return mousePos.Delta;
            }
        }

        public override IEnumerable<InputUpdate> GetChildren()
        {
            yield return mousePos;
            yield return windowActive;
        }

        public override string ToString()
        {
            return $"MouseDelta ({Value})";
        }

        public void PreventMouseMovement(int forNextUpdates = 1)
        {
            if (ignoreMouseMovementForNextUpdates < forNextUpdates)
            {
                ignoreMouseMovementForNextUpdates = forNextUpdates;
            }
        }
    }
}
