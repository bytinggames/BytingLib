using Microsoft.Xna.Framework.Input;

namespace BytingLib
{
    public partial class KeyInput : IUpdate
    {
        private readonly Func<KeyboardState> getState;

        private KeyboardState previousState;
        private KeyboardState currentState;
        private int frame = -1;

        public delegate void StateChanged(int frame, KeyboardState current, KeyboardState previous);
        public event StateChanged? OnStateChanged;

        public KeyInput(Func<KeyboardState> getState)
        {
            this.getState = getState;
        }

        public void Update()
        {
            UpdateUsingState(getState());
        }

        private void UpdateUsingState(KeyboardState keyboardState)
        {
            frame++;
            
            previousState = currentState;
            currentState = keyboardState;

            if (OnStateChanged != null
                && currentState != previousState)
                OnStateChanged?.Invoke(frame, currentState, previousState);
        }

        public bool NumLockActive => currentState.NumLock;
        public bool CapsLockActive => currentState.CapsLock;

        public IKey GetKey(Keys key)
        {
            bool downNow = currentState.IsKeyDown(key);
            bool downPreviously = previousState.IsKeyDown(key);
            return new Key(downNow, downNow != downPreviously);
        }

        public KeyboardState GetState() => currentState;
        public KeyboardState GetStatePrevious() => previousState;
    }
}