using Microsoft.Xna.Framework.Input;

namespace BytingLib
{
    /// <summary>Sets "Down" to true, even when releasing keys or moving the mouse.</summary>
    public class BoolAnyInput : InputBool<BoolAnyInputState>
    {
        public override BoolAnyInputState CreateState(InputUpdater updater) => new BoolAnyInputState(updater);

        protected override bool CalculateValue(FullInput input, BoolAnyInputState state)
        {
            return state.CalculateValue(input);
        }

        public override IEnumerable<Input> GetChildren()
        {
            yield break;
        }
    }

    public class BoolAnyInputState : InputBoolState
    {
        KeyboardState lastKeyState;
        MouseState lastMouseState;
        GamePadState lastGamePadState;

        public BoolAnyInputState(InputUpdater updater) : base(updater)
        {
        }

        public bool CalculateValue(FullInput input)
        {
            bool inputChanged = input.MouseState != lastMouseState
                || input.KeyState != lastKeyState
                || input.GamePadState != lastGamePadState;

            lastKeyState = input.KeyState;
            lastMouseState = input.MouseState;
            lastGamePadState = input.GamePadState;

            return inputChanged;
        }
    }

}
