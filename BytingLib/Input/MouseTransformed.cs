using Microsoft.Xna.Framework.Input;

namespace BytingLib
{
    public class MouseTransformed
    {
        private readonly Func<MouseState> getState;
        private readonly Func<Matrix> getTransform;

        public MouseTransformed(Func<MouseState> getState, Func<Matrix> getTransform)
        {
            this.getState = getState;
            this.getTransform = getTransform;
        }

        public MouseState GetState()
        {
            MouseState state = getState();

            Matrix transform = getTransform();
            transform = Matrix.Invert(transform);
            Vector2 newMousePos = Vector2.Transform(state.Position.ToVector2(), transform);

            return new MouseState(
                (int)MathF.Round(newMousePos.X),
                (int)MathF.Round(newMousePos.Y),
                state.ScrollWheelValue,
                state.LeftButton,
                state.MiddleButton,
                state.RightButton,
                state.XButton1,
                state.XButton2,
                state.HorizontalScrollWheelValue
            );
        }
    }
}
