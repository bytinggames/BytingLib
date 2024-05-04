using System.Text.Json;
using System.Text.Json.Serialization;

namespace BytingLib
{
    public class ValueEventStringJsonConverter : JsonConverter<ValueEvent<string>>
    {
        public override ValueEvent<string> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.StartObject)
            {
                SimpleJsonObjectReader.BeginReadObject(ref reader, "Value", [JsonTokenType.String]);
                var val = new ValueEvent<string>(reader.GetString() ?? "");
                SimpleJsonObjectReader.EndReadObject(ref reader);
                return val;
            }
            else
            {
                return new ValueEvent<string>(reader.GetString() ?? "");
            }
        }

        public override void Write(Utf8JsonWriter writer, ValueEvent<string> value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.Value);
        }
    }
}
