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

        public void RefreshStateForCurrentFrame()
        {
            currentState = getState();
        }

        private void UpdateUsingState(KeyboardState keyboardState)
        {
            frame++;
            
            previousState = currentState;
            currentState = keyboardState;

            if (OnStateChanged != null
                && currentState != previousState)
            {
                OnStateChanged?.Invoke(frame, currentState, previousState);
            }
        }

        public bool NumLockActive => currentState.NumLock;
        public bool CapsLockActive => currentState.CapsLock;

        public IKey GetKey(Keys key)
        {
            bool downNow = currentState.IsKeyDown(key);
            bool downPreviously = previousState.IsKeyDown(key);
            return new Key(downNow, downNow != downPreviously);
        }
        public IKey GetKeyAny(params Keys[] keys) => GetKeyAnyFromIList(keys);
        public IKey GetKeyAnyFromIList(IList<Keys> keys)
        {
            bool downNow = keys.Any(f => currentState.IsKeyDown(f));
            bool downPreviously = keys.Any(f => previousState.IsKeyDown(f));
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
                {
                    pressedKeys.Add(downNow[i]);
                }
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
                {
                    releasedKeys.Add(downPrevious[i]);
                }
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

        public IKey Number(int number)
        {
            switch (number)
            {
                case 0: return Any0;
                case 1: return Any1;
                case 2: return Any2;
                case 3: return Any3;
                case 4: return Any4;
                case 5: return Any5;
                case 6: return Any6;
                case 7: return Any7;
                case 8: return Any8;
                case 9: return Any9;
                default:
                    throw new ArgumentException(nameof(number) + " " + number + " is not supported");
            }
        }
    }
}