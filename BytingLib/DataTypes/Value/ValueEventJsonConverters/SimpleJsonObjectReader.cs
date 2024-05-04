using System.Text.Json;

namespace BytingLib
{
    internal static class SimpleJsonObjectReader
    {
        public static void BeginReadObject(ref Utf8JsonReader reader, string propertyName, JsonTokenType[] allowedTypes)
        {
            reader.Read();
            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new Exception($"json token was not a PropertyName");
            }

            string? propName = reader.GetString();
            if (propName != propertyName)
            {
                throw new Exception($"wrong property name: {propName} != {propertyName}");
            }

            reader.Read();
            int i;
            for (i = 0; i < allowedTypes.Length; i++)
            {
                if (allowedTypes[i] == reader.TokenType)
                {
                    break;
                }
            }
            if (i == allowedTypes.Length)
            {
                throw new Exception($"json token type was not one of the given allowed types");
            }
        }

        public static void EndReadObject(ref Utf8JsonReader reader)
        {
            reader.Read();
            if (reader.TokenType != JsonTokenType.EndObject)
            {
                throw new Exception("object didn't end with EndObject token");
            }
        }

    }
}
