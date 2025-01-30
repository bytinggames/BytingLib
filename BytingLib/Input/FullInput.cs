
using Microsoft.Xna.Framework.Input;

namespace BytingLib
{
    public record struct FullInput(
        MouseState MouseState, 
        KeyboardState KeyState, 
        GamePadState GamePadState, 
        MetaInputState MetaState,
        Int2 WindowResolution
    );
}
