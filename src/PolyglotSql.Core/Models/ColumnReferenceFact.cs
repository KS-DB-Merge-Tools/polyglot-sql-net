using System.Text.Json.Serialization;

namespace PolyglotSql.Models
{
    public record ColumnReferenceFact
    {
        [JsonPropertyName("sourceName")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string SourceName { get; set; }

        [JsonPropertyName("sourceAlias")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string SourceAlias { get; set; }

        [JsonPropertyName("sourceKind")]
        public SourceKind SourceKind { get; set; }

        [JsonPropertyName("table")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Table { get; set; }

        [JsonPropertyName("column")]
        public string Column { get; set; }

        [JsonPropertyName("unqualified")]
        public bool Unqualified { get; set; }

        [JsonPropertyName("confidence")]
        public ReferenceConfidence Confidence { get; set; }
    }
}
