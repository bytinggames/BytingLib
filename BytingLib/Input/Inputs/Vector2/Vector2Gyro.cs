namespace BytingLib
{
    /// <summary>Without mouse acceleration</summary>
    public class Vector2Gyro : InputVector2<Vector2GyroState>
    {
        private readonly InputBool windowActive = new BoolWindowActive();

        protected override Vector2 CalculateValue(FullInput input, Vector2GyroState state)
        {
            if (windowActive.GetState(state.Updater).Pressed)
            {
                state.PreventGyroMovement();
            }

            if (state.IgnoreGyroMovementForNextUpdates-- > 0)
            {
                return Vector2.Zero;
            }
            else
            {
                return new Vector2(input.GamePadState.Sensors.Main.Gyro.Y, input.GamePadState.Sensors.Main.Gyro.X);
            }
        }

        public override IEnumerable<Input> GetChildren()
        {
            yield return windowActive;
        }

        public override string ToString()
        {
            return $"Gyro";
        }

        public override Vector2GyroState CreateState(InputUpdater updater)
        {
            return new Vector2GyroState(updater);
        }
    }

    public class Vector2GyroState : InputVector2State
    {
        public int IgnoreGyroMovementForNextUpdates { get; set; } = 0; // if the player moves the mouse while a level is loading, it shouldn't affect the view direction

        public Vector2GyroState(InputUpdater updater) : base(updater)
        {
        }

        public void PreventGyroMovement(int forNextUpdates = 1)
        {
            if (IgnoreGyroMovementForNextUpdates < forNextUpdates)
            {
                IgnoreGyroMovementForNextUpdates = forNextUpdates;
            }
        }
    }
}
