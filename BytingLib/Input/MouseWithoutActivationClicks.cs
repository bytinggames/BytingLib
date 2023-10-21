using Microsoft.Xna.Framework.Input;

namespace BytingLib
{
    public class MouseWithoutActivationClicks
    {
        private readonly Func<MouseState> getState;

        private bool blockLeftButton = false;

        public MouseWithoutActivationClicks(Func<MouseState> getState, Action<EventHandler<EventArgs>> subscribeToOnGameActivated)
        {
            this.getState = getState;

            subscribeToOnGameActivated(Game_Activated);
        }

        private void Game_Activated(object? sender, EventArgs e)
        {
            var mouseState = getState();
            if (mouseState.LeftButton == ButtonState.Pressed)
            {
                blockLeftButton = true;
            }
        }

        public MouseState GetState()
        {
            MouseState state = getState();

            if (blockLeftButton)
            {
                if (state.LeftButton == ButtonState.Released)
                {
                    blockLeftButton = false;
                }
                else
                {
                    // manipulate input so that left button isn't pressed
                    state.LeftButton = ButtonState.Released;
                }
            }

            return state;
        }
    }
}
