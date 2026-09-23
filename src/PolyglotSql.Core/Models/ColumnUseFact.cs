using System.Text.Json.Serialization;

namespace PolyglotSql.Models
{
    // One containing expression (join/filter/group/... clause), with references in occurrence order.
    // Paths are deterministic within an analysis, not persistent IDs across SQL edits.
    public record ColumnUseFact
    {
        [JsonPropertyName("context")]
        public ColumnUseContext Context { get; set; }

        [JsonPropertyName("scopePath")]
        public string ScopePath { get; set; }

        [JsonPropertyName("expressionPath")]
        public string ExpressionPath { get; set; }

        // Dialect-rendered SQL; not necessarily the original source substring.
        [JsonPropertyName("expressionSql")]
        public string ExpressionSql { get; set; }

        [JsonPropertyName("span")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public QuerySourceSpan Span { get; set; }

        [JsonPropertyName("references")]
        public ColumnUseReferenceFact[] References { get; set; }
    }
}
