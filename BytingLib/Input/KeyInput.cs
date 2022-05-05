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
        public IKey GetKeyAny(params Keys[] keys)
        {
            bool downNow = keys.Any(f => currentState.IsKeyDown(f));
            bool downPreviously = keys.All(f => previousState.IsKeyDown(f));
            return new Key(downNow, downNow != downPreviously);
        }

        public KeyboardState GetState() => currentState;
        public KeyboardState GetStatePrevious() => previousState;

        public List<Keys> GetPressedKeys()
        {
            Keys[] downNow = currentState.GetPressedKeys();
            Keys[] downPrevious = previousState.GetPressedKeys();

            List<Keys> pressedKeys = new List<Keys>();
            for (int i = 0; i < downNow.Length; i++)
            {
                if (!downPrevious.Contains(downNow[i]))
                    pressedKeys.Add(downNow[i]);
            }
            return pressedKeys;
        }
        public IList<Keys> GetDownKeys()
        {
            return currentState.GetPressedKeys();
        }
        public List<Keys> GetReleasedKeys()
        {
            Keys[] downNow = currentState.GetPressedKeys();
            Keys[] downPrevious = previousState.GetPressedKeys();

            List<Keys> releasedKeys = new List<Keys>();
            for (int i = 0; i < downPrevious.Length; i++)
            {
                if (!downNow.Contains(downPrevious[i]))
                    releasedKeys.Add(downPrevious[i]);
            }
            return releasedKeys;
        }

        public IKey Shift => GetKeyAny(Keys.LeftShift, Keys.RightShift);
        public IKey Control => GetKeyAny(Keys.LeftControl, Keys.RightControl);
        public IKey Alt => GetKeyAny(Keys.LeftAlt, Keys.RightAlt);
        public IKey Any0 => GetKeyAny(Keys.D0, Keys.NumPad0);
        public IKey Any1 => GetKeyAny(Keys.D1, Keys.NumPad1);
        public IKey Any2 => GetKeyAny(Keys.D2, Keys.NumPad2);
        public IKey Any3 => GetKeyAny(Keys.D3, Keys.NumPad3);
        public IKey Any4 => GetKeyAny(Keys.D4, Keys.NumPad4);
        public IKey Any5 => GetKeyAny(Keys.D5, Keys.NumPad5);
        public IKey Any6 => GetKeyAny(Keys.D6, Keys.NumPad6);
        public IKey Any7 => GetKeyAny(Keys.D7, Keys.NumPad7);
        public IKey Any8 => GetKeyAny(Keys.D8, Keys.NumPad8);
        public IKey Any9 => GetKeyAny(Keys.D9, Keys.NumPad9);
    }
}