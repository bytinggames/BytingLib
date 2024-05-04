using System.Text.Json;
using System.Text.Json.Serialization;

namespace BytingLib
{
    public class ValueEventBoolJsonConverter : JsonConverter<ValueEvent<bool>>
    {
        public override ValueEvent<bool> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.StartObject)
            {
                SimpleJsonObjectReader.BeginReadObject(ref reader, "Value", [JsonTokenType.True, JsonTokenType.False]);
                var val = new ValueEvent<bool>(reader.GetBoolean());
                SimpleJsonObjectReader.EndReadObject(ref reader);
                return val;
            }
            else
            {
                return new ValueEvent<bool>(reader.GetBoolean());
            }
        }

        public override void Write(Utf8JsonWriter writer, ValueEvent<bool> value, JsonSerializerOptions options)
        {
            writer.WriteBooleanValue(value.Value);
        }
    }
}
