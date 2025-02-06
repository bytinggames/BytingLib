using System.Text.Json.Serialization;

namespace BytingLib
{
    [JsonDerivedType(typeof(Vector2Delta), "Vector2Delta")]
    [JsonDerivedType(typeof(Vector2FromBools), "Vector2FromBools")]
    [JsonDerivedType(typeof(Vector2Mouse), "Vector2Mouse")]
    [JsonDerivedType(typeof(Vector2MouseMoveLinear), "Vector2MouseMoveLinear")]
    [JsonDerivedType(typeof(Vector2Transform), "Vector2Transform")]
    public abstract partial class InputVector2 : Input
    {
    }
}
