using Microsoft.Xna.Framework.Input;
using System.Text.Json.Serialization;

namespace BytingLib
{
    [JsonDerivedType(typeof(FloatFromBool), "FloatFromBool")]
    public abstract partial class InputFloat : Input
    {
        public static implicit operator InputFloat(Keys key) => new FloatFromBool(key);
        public static implicit operator InputFloat(MouseButton mouseButton) => new FloatFromBool(mouseButton);
        public static implicit operator InputFloat(Buttons gamePadButton) => new FloatFromBool(gamePadButton);
    }
}
