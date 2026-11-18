using System;
using System.Text.Json.Serialization;

namespace PolyglotSql.Models
{
    // One resolved dependency of an original column occurrence.
    // Native type flattens ColumnReferenceFact into this object (#[serde(flatten)]) and adds `span`,
    // so the ColumnReferenceFact properties are repeated here to match the flat JSON.
    public record ColumnUseReferenceFact
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

        // Path metadata of ColumnReferenceFact. Native only fills it for projection upstream
        // references, so on column uses these normally keep their defaults.
        [JsonPropertyName("pathTransformKind")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public TransformKind? PathTransformKind { get; set; }

        [JsonPropertyName("pathFunctions")]
        public string[] PathFunctions { get; set; } = Array.Empty<string>();

        [JsonPropertyName("pathWindow")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public bool PathWindow { get; set; }

        // Span of this use (not of the upstream column's definition).
        [JsonPropertyName("span")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public QuerySourceSpan Span { get; set; }
    }
}
