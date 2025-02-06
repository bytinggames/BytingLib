using Microsoft.Xna.Framework.Input;

namespace BytingLib
{
    public class BoolMouse(MouseButton mouseButton) : InputBoolSimple
    {
        public MouseButton MouseButton { get; } = mouseButton;

        protected override bool CalculateValue(FullInput input, InputBoolState state)
        {
            var s = input.MouseState;
            switch (MouseButton)
            {
                case MouseButton.Left:
                    return s.LeftButton == ButtonState.Pressed;
                case MouseButton.Right:
                    return s.RightButton == ButtonState.Pressed;
                case MouseButton.Middle:
                    return s.MiddleButton == ButtonState.Pressed;
                case MouseButton.Button1:
                    return s.XButton1 == ButtonState.Pressed;
                case MouseButton.Button2:
                    return s.XButton2 == ButtonState.Pressed;
                default:
                    throw new NotImplementedException();
            }
        }

        public override string ToString()
        {
            return MouseButton + " Mouse";
        }
    }
}
