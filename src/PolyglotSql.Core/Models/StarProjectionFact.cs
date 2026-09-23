using System.Text.Json.Serialization;

namespace PolyglotSql.Models
{
    // Compact fact about one original star projection.
    public record StarProjectionFact
    {
        [JsonPropertyName("index")]
        public int Index { get; set; }

        [JsonPropertyName("table")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Table { get; set; }

        [JsonPropertyName("expandedColumns")]
        public string[] ExpandedColumns { get; set; }
    }
}
