using System.Text.Json;
using System.Text.Json.Serialization;

namespace BytingLib
{
    public class ValueEventFloatJsonConverter : JsonConverter<ValueEvent<float>>
    {
        public override ValueEvent<float> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.StartObject)
            {
                SimpleJsonObjectReader.BeginReadObject(ref reader, "Value", [JsonTokenType.Number]);
                var val = new ValueEvent<float>(reader.GetSingle());
                SimpleJsonObjectReader.EndReadObject(ref reader);
                return val;
            }
            else
            {
                return new ValueEvent<float>(reader.GetSingle());
            }
        }

        public override void Write(Utf8JsonWriter writer, ValueEvent<float> value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(value.Value);
        }
    }
}
