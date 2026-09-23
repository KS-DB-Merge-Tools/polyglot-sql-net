using System.Text.Json.Serialization;

namespace PolyglotSql.Models
{
    // Compact fact about a function-like projection transform.
    public record TransformFunctionFact
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("literalArgs")]
        public string[] LiteralArgs { get; set; }

        [JsonPropertyName("columnArgs")]
        public ColumnReferenceFact[] ColumnArgs { get; set; }
    }
}
