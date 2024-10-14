using System.Text.Json.Nodes;

namespace BytingLib
{
    public static class JsonNodeExtension
    {
        public static T? TryGetValueFromString<T>(this JsonNode node) where T : struct
        {
            return (T?)Convert.ChangeType(node.GetValue<string>(), typeof(T));
        }
    }
}
