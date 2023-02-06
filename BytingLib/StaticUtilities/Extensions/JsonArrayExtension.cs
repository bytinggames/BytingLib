using System.Text.Json.Nodes;

namespace BytingLib
{
    public static class JsonArrayExtension
    {
        public static Vector2? GetVector2(this JsonArray arr)
        {
            if (arr.Count < 2)
                return null;
            return new Vector2(arr[0]!.GetValue<float>(), arr[1]!.GetValue<float>());
        }
        public static Vector3? GetVector3(this JsonArray arr)
        {
            if (arr.Count < 3)
                return null;
            return new Vector3(arr[0]!.GetValue<float>(), arr[1]!.GetValue<float>(), arr[2]!.GetValue<float>());
        }
        public static Vector4? GetVector4(this JsonArray arr)
        {
            if (arr.Count < 4)
                return null;
            return new Vector4(arr[0]!.GetValue<float>(), arr[1]!.GetValue<float>(), arr[2]!.GetValue<float>(), arr[3]!.GetValue<float>());
        }
    }
}
