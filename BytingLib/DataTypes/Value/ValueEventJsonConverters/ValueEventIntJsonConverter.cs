using System.Text.Json;
using System.Text.Json.Serialization;

namespace BytingLib
{
    public class ValueEventIntJsonConverter : JsonConverter<ValueEvent<int>>
    {
        public override ValueEvent<int> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.StartObject)
            {
                SimpleJsonObjectReader.BeginReadObject(ref reader, "Value", [JsonTokenType.Number]);
                var val = new ValueEvent<int>(reader.GetInt32());
                SimpleJsonObjectReader.EndReadObject(ref reader);
                return val;
            }
            else
            {
                return new ValueEvent<int>(reader.GetInt32());
            }
        }

        public override void Write(Utf8JsonWriter writer, ValueEvent<int> value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(value.Value);
        }
    }
}
