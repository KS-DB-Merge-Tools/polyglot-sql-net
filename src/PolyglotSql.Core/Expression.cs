using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PolyglotSql
{
    [JsonConverter(typeof(ExpressionJsonConverter))]
    public readonly struct Expression
    {
        public readonly JsonElement Json;

        internal Expression(JsonElement json)
        {
            Json = json;
        }

        public string ToJsonString() => Json.ValueKind != JsonValueKind.Undefined ? Json.GetRawText() : "{}";
    }

    public class ExpressionJsonConverter : JsonConverter<Expression>
    {
        public override Expression Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using var doc = JsonDocument.ParseValue(ref reader);
            return new Expression(doc.RootElement.Clone());
        }

        public override void Write(Utf8JsonWriter writer, Expression value, JsonSerializerOptions options)
        {
            if (value.Json.ValueKind == JsonValueKind.Undefined || value.Json.ValueKind == JsonValueKind.Null)
                writer.WriteNullValue();
            else
                value.Json.WriteTo(writer);
        }
    }
}
