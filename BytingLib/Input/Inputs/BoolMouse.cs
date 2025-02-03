using Microsoft.Xna.Framework.Input;

namespace BytingLib
{
    [InputShortcut("Mouse")]
    public class BoolMouse(MouseButton mouseButton) : BoolInput
    {
        public override IEnumerable<InputUpdate> GetChildren()
        {
            yield break;
        }

        protected override bool CalculateValue(FullInput input)
        {
            var state = input.MouseState;
            switch (mouseButton)
            {
                case MouseButton.Left:
                    return state.LeftButton == ButtonState.Pressed;
                case MouseButton.Right:
                    return state.RightButton == ButtonState.Pressed;
                case MouseButton.Middle:
                    return state.MiddleButton == ButtonState.Pressed;
                case MouseButton.Button1:
                    return state.XButton1 == ButtonState.Pressed;
                case MouseButton.Button2:
                    return state.XButton2 == ButtonState.Pressed;
                default:
                    throw new NotImplementedException();
            }
        }

        public override string ToString()
        {
            return mouseButton + " Mouse";
        }
    }
}
