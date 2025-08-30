using System.Text.Json;
using System.Text.Json.Serialization;

namespace BytingLib
{
    public class ValueEventColorJsonConverter : JsonConverter<ValueEvent<Color>>
    {
        public override ValueEvent<Color> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.StartObject)
            {
                SimpleJsonObjectReader.BeginReadObject(ref reader, "Value", [JsonTokenType.True, JsonTokenType.False]);
                var val = new ValueEvent<Color>(new Color(reader.GetString()));
                SimpleJsonObjectReader.EndReadObject(ref reader);
                return val;
            }
            else
            {
                return new ValueEvent<Color>(new Color(reader.GetString()));
            }
        }

        public override void Write(Utf8JsonWriter writer, ValueEvent<Color> value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.Value.ToHex());
        }
    }
}
