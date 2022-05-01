using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace BytingLib
{
    public partial class GamePadInput : IUpdate
    {
        private readonly Func<GamePadState> getState;

        private GamePadState previousState;
        private GamePadState currentState;

        public delegate void StateChanged(GamePadState current, GamePadState previous);
        public event StateChanged? OnStateChanged;

        public GamePadInput(Func<GamePadState> getState)
        {
            this.getState = getState;
        }

        public void Update()
        {
            UpdateUsingState(getState());
        }

        private void UpdateUsingState(GamePadState keyboardState)
        {
            previousState = currentState;
            currentState = keyboardState;

            if (OnStateChanged != null
                && currentState != previousState)
                OnStateChanged?.Invoke(currentState, previousState);
        }

        public bool IsConnected => currentState.IsConnected;
        public bool IsConnectedStart => currentState.IsConnected && !previousState.IsConnected;
        public bool IsConnectedEnd => !currentState.IsConnected && previousState.IsConnected;
        public int PacketNumber => currentState.PacketNumber;
        public Vector2 LeftThumbStick => new Vector2(currentState.ThumbSticks.Left.X, -currentState.ThumbSticks.Left.Y);
        public Vector2 RightThumbStick => new Vector2(currentState.ThumbSticks.Right.X, -currentState.ThumbSticks.Right.Y);
        public float LeftTriggerAmount => currentState.Triggers.Left;
        public float RightTriggerAmount => currentState.Triggers.Right;
        public Int2 DPad
        {
            get
            {
                Int2 dir = Int2.Zero;
                if (currentState.DPad.Left == ButtonState.Pressed)
                    dir.X--;
                if (currentState.DPad.Right == ButtonState.Pressed)
                    dir.X++;
                if (currentState.DPad.Up == ButtonState.Pressed)
                    dir.Y--;
                if (currentState.DPad.Down == ButtonState.Pressed)
                    dir.Y++;
                return dir;
            }
        }

        public IKey GetKey(Buttons button)
        {
            bool downNow = currentState.IsButtonDown(button);
            bool downPreviously = previousState.IsButtonDown(button);
            return new Key(downNow, downNow != downPreviously);
        }

        public GamePadState GetState() => currentState;
        public GamePadState GetStatePrevious() => previousState;
    }
}
