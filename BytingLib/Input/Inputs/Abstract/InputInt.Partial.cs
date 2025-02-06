using System.Text.Json.Serialization;

namespace BytingLib
{
    [JsonDerivedType(typeof(IntIncrement), "IntIncrement")]
    [JsonDerivedType(typeof(IntMouseWheel), "IntMouseWheel")]
    [JsonDerivedType(typeof(IntNumber), "IntNumber")]
    [JsonDerivedType(typeof(IntOnChange), "IntOnChange")]
    public abstract partial class InputInt : Input
    {
    }
}
