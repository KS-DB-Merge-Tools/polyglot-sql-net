using System;
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

        // Strongest transform on the lineage path from this reference to the projection output
        // (through CTEs, derived tables and set-operation branches). Only projection upstream references carry it.
        [JsonPropertyName("pathTransformKind")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public TransformKind? PathTransformKind { get; set; }

        // Function names on the same path, from the output to the source, in the dialect spelling.
        // Native omits the key when the list is empty.
        [JsonPropertyName("pathFunctions")]
        public string[] PathFunctions { get; set; } = Array.Empty<string>();

        // True when a step on the path is a window function.
        [JsonPropertyName("pathWindow")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public bool PathWindow { get; set; }
    }
}
