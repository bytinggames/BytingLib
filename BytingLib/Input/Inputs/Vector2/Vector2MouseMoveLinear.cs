namespace BytingLib
{
    /// <summary>Without mouse acceleration</summary>
    public class Vector2MouseMoveLinear : InputVector2<Vector2MouseMoveLinearState>
    {
        private readonly InputVector2 mousePos = new Vector2Mouse();
        private readonly InputBool windowActive = new BoolWindowActive();

        protected override Vector2 CalculateValue(FullInput input, Vector2MouseMoveLinearState state)
        {
            if (windowActive.GetState(state.Updater).Pressed)
            {
                state.PreventMouseMovement();
            }

            if (state.IgnoreMouseMovementForNextUpdates-- > 0)
            {
                return Vector2.Zero;
            }
            else
            {
                return mousePos.GetState(state.Updater).Delta;
            }
        }

        public override IEnumerable<Input> GetChildren()
        {
            yield return mousePos;
            yield return windowActive;
        }

        public override string ToString()
        {
            return $"Mouse";
        }

        public override Vector2MouseMoveLinearState CreateState(InputUpdater updater)
        {
            return new Vector2MouseMoveLinearState(updater);
        }
    }

    public class Vector2MouseMoveLinearState : InputVector2State
    {
        public int IgnoreMouseMovementForNextUpdates { get; set; } = 0; // if the player moves the mouse while a level is loading, it shouldn't affect the view direction
        
        public Vector2MouseMoveLinearState(InputUpdater updater) : base(updater)
        {
        }

        public void PreventMouseMovement(int forNextUpdates = 1)
        {
            if (IgnoreMouseMovementForNextUpdates < forNextUpdates)
            {
                IgnoreMouseMovementForNextUpdates = forNextUpdates;
            }
        }
    }
}
