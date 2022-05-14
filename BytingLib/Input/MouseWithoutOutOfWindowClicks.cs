using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace BytingLib
{
    public class MouseWithoutOutOfWindowClicks
    {
        private readonly Func<MouseState> getState;
        private readonly IGetResolution getResolution;

        private bool blockLeftButton = false;
        private MouseState previousMouseState;

        public MouseWithoutOutOfWindowClicks(Func<MouseState> getState, IGetResolution getResolution)
        {
            this.getState = getState;
            this.getResolution = getResolution;
        }

        public MouseState GetState()
        {
            MouseState state = getState();

            if (blockLeftButton)
            {
                if (state.LeftButton == ButtonState.Released)
                    blockLeftButton = false;
            }
            else if (state.LeftButton == ButtonState.Pressed
                && previousMouseState.LeftButton == ButtonState.Released) // only prevent mouse click if clicked this frame
            {
                Int2 res = getResolution.GetResolution();

                Point pos = state.Position;
                if (pos.X < 0
                    || pos.Y < 0
                    || pos.X >= res.X
                    || pos.Y >= res.Y)
                {
                    // prevent mouse click
                    blockLeftButton = true;
                    // manipulate input so that left button isn't pressed
                    state.LeftButton = ButtonState.Released;
                }
            }

            previousMouseState = state;

            return state;
        }
    }
}
