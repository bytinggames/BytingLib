using Microsoft.Xna.Framework.Input;
using System.Text.Json.Serialization;

namespace BytingLib
{
    [JsonDerivedType(typeof(BoolKey), "BoolKey")]
    [JsonDerivedType(typeof(BoolAnyInput), "BoolAnyInput")]
    [JsonDerivedType(typeof(BoolMouse), "BoolMouse")]
    [JsonDerivedType(typeof(BoolGamePad), "BoolGamePad")]
    [JsonDerivedType(typeof(BoolAnd), "BoolAnd")]
    [JsonDerivedType(typeof(BoolOr), "BoolOr")]
    [JsonDerivedType(typeof(BoolShift), "BoolShift")]
    [JsonDerivedType(typeof(BoolAlt), "BoolAlt")]
    [JsonDerivedType(typeof(BoolCtrl), "BoolCtrl")]
    [JsonDerivedType(typeof(BoolFalse), "BoolFalse")]
    [JsonDerivedType(typeof(BoolMouseWheel), "BoolMouseWheel")]
    [JsonDerivedType(typeof(BoolNot), "BoolNot")]
    [JsonDerivedType(typeof(BoolWindowActive), "BoolWindowActive")]
    [JsonDerivedType(typeof(BoolOnPress), "BoolOnPress")]
    [JsonDerivedType(typeof(BoolDelayed), "BoolDelayed")]
    public abstract partial class InputBool : Input
    {
        public static implicit operator InputBool(Keys key) => new BoolKey(key);
        public static implicit operator InputBool(MouseButton mouseButton) => new BoolMouse(mouseButton);
        public static implicit operator InputBool(Buttons gamePadButton) => new BoolGamePad(gamePadButton);
    }
}
