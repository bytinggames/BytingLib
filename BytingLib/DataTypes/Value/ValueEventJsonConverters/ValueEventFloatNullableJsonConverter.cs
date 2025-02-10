using System.Text.Json;
using System.Text.Json.Serialization;

namespace BytingLib
{
    public class ValueEventFloatNullableJsonConverter : JsonConverter<ValueEvent<float?>>
    {
        public override ValueEvent<float?> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.StartObject)
            {
                SimpleJsonObjectReader.BeginReadObject(ref reader, "Value", [JsonTokenType.Number]);
                float? floatValue = reader.GetSingle();

                var val = new ValueEvent<float?>(floatValue);
                SimpleJsonObjectReader.EndReadObject(ref reader);
                return val;
            }
            else if (reader.TokenType == JsonTokenType.String)
            {
                return new ValueEvent<float?>(null);
            }
            else
            {
                return new ValueEvent<float?>(reader.GetSingle());
            }
        }

        public override void Write(Utf8JsonWriter writer, ValueEvent<float?> value, JsonSerializerOptions options)
        {
            if (value.Value == null)
            {
                writer.WriteStringValue("null");
            }
            else
            {
                writer.WriteNumberValue(value.Value.Value);
            }
        }
    }
}
