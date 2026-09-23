using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PolyglotSql.Models
{
    // Builder program for polyglot_build. Like Expression, this is a thin wrapper over JSON:
    // the native plan is a large tagged-union tree ({"base": {...}, "operations": [...]}),
    // so it is not modeled as C# classes.
    [JsonConverter(typeof(BuilderPlanJsonConverter))]
    public record BuilderPlan
    {
        public JsonElement Json { get; private set; }

        internal BuilderPlan(JsonElement json)
        {
            Json = json;
        }

        public static BuilderPlan FromJson(string json)
        {
            if (json == null)
                throw new ArgumentNullException(nameof(json));

            using var doc = JsonDocument.Parse(json);
            return new BuilderPlan(doc.RootElement.Clone());
        }

        public string ToJsonString() => Json.ValueKind != JsonValueKind.Undefined ? Json.GetRawText() : "{}";
    }

    public class BuilderPlanJsonConverter : JsonConverter<BuilderPlan>
    {
        public override BuilderPlan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using var doc = JsonDocument.ParseValue(ref reader);
            return new BuilderPlan(doc.RootElement.Clone());
        }

        public override void Write(Utf8JsonWriter writer, BuilderPlan value, JsonSerializerOptions options)
        {
            if (value.Json.ValueKind == JsonValueKind.Undefined || value.Json.ValueKind == JsonValueKind.Null)
                writer.WriteNullValue();
            else
                value.Json.WriteTo(writer);
        }
    }
}
