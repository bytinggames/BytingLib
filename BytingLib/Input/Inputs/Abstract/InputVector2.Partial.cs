using System.Text.Json.Serialization;

namespace BytingLib
{
    [JsonDerivedType(typeof(Vector2Delta), "Vector2Delta")]
    [JsonDerivedType(typeof(Vector2FromBools), "Vector2FromBools")]
    [JsonDerivedType(typeof(Vector2Mouse), "Vector2Mouse")]
    [JsonDerivedType(typeof(Vector2MouseMoveLinear), "Vector2MouseMoveLinear")]
    [JsonDerivedType(typeof(Vector2Gyro), "Vector2Gyro")]
    [JsonDerivedType(typeof(Vector2Transform), "Vector2Transform")]
    [JsonDerivedType(typeof(Vector2PressedPerAxis), "Vector2PressedPerAxis")]
    [JsonDerivedType(typeof(Vector2PressedCircular), "Vector2PressedCircular")]
    [JsonDerivedType(typeof(Vector2CircularDeadzone), "Vector2CircularDeadzone")]
    [JsonDerivedType(typeof(Vector2GamePadStick), "Vector2GamePadStick")]
    [JsonDerivedType(typeof(Vector2Multiply), "Vector2Multiply")]
    [JsonDerivedType(typeof(Vector2Multiply2), "Vector2Multiply2")]
    [JsonDerivedType(typeof(Vector2SquareDeadzone), "Vector2SquareDeadzone")]
    [JsonDerivedType(typeof(Vector2StickPow), "Vector2StickPow")]
    [JsonDerivedType(typeof(Vector2Sum), "Vector2Sum")]
    [JsonDerivedType(typeof(Vector2GamePadStickCustom), "Vector2GamePadStickCustom")]
    [JsonDerivedType(typeof(Vector2MaxLength), "Vector2MaxLength")]
    public abstract partial class InputVector2 : Input
    {
    }

    public partial class Vector2Switch
    {
        public override string ToString()
        {
            if (OnFalse is Vector2Const c && c.Value == Vector2.Zero)
            {
                return $"{Condition} + {OnTrue}";
            }
            else
            {
                return $"{Condition} ? {OnTrue} : {OnFalse}";
            }
        }
    }
}
