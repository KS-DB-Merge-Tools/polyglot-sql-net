using System.Text.Json.Serialization;

namespace PolyglotSql.Models
{
    public record AnalyzeQueryOptions
    {
        [JsonPropertyName("dialect")]
        [JsonConverter(typeof(DialectJsonConverter))]
        public Dialect Dialect { get; set; } = Dialect.Generic;

        [JsonPropertyName("schema")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ValidationSchema Schema { get; set; }
    }
}
