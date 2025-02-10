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
                var foundType = SimpleJsonObjectReader.BeginReadObject(ref reader, "Value", [JsonTokenType.Number, JsonTokenType.Null]);
                float? floatValue = null;
                if (foundType == JsonTokenType.Number)
                {
                    floatValue = reader.GetSingle();
                }
                
                var val = new ValueEvent<float?>(floatValue);
                SimpleJsonObjectReader.EndReadObject(ref reader);
                return val;
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
                writer.WriteNullValue();
            }
            else
            {
                writer.WriteNumberValue(value.Value.Value);
            }
        }
    }
}
