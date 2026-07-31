using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace PolyglotSql.Models
{
    [JsonConverter(typeof(ExpressionJsonConverter))]
    public record Expression
    {
        public JsonElement Json { get; private set; }

        internal Expression(JsonElement json)
        {
            Json = json;
        }

        public string ToJsonString() => Json.ValueKind != JsonValueKind.Undefined ? Json.GetRawText() : "{}";

        public void TransformAll(Action<JsonNode> transform)
        {
            var rootNode = JsonNode.Parse(this.ToJsonString());
            TransformRecursive(rootNode, transform);
            var newJsonString = rootNode.ToJsonString();
            var doc = JsonDocument.Parse(newJsonString);
            this.Json = doc.RootElement.Clone();
        }

        private static void TransformRecursive(JsonNode node, Action<JsonNode> transform)
        {
            if (node == null) return;

            transform(node);

            if (node is JsonObject obj)
            {
                foreach (var property in obj)
                {
                    TransformRecursive(property.Value, transform);
                }
            }
            else if (node is JsonArray array)
            {
                foreach (var item in array)
                {
                    TransformRecursive(item, transform);
                }
            }
        }
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
