using System.Text.Json.Serialization;

namespace PolyglotSql.Models
{
    public record ProjectionFact
    {
        [JsonPropertyName("index")]
        public int Index { get; set; }

        [JsonPropertyName("name")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Name { get; set; }

        [JsonPropertyName("isStar")]
        public bool IsStar { get; set; }

        [JsonPropertyName("starTable")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string StarTable { get; set; }

        [JsonPropertyName("transformKind")]
        public TransformKind TransformKind { get; set; }

        [JsonPropertyName("transformFunction")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public TransformFunctionFact TransformFunction { get; set; }

        [JsonPropertyName("castType")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string CastType { get; set; }

        [JsonPropertyName("typeHint")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string TypeHint { get; set; }

        [JsonPropertyName("nullability")]
        public ProjectionNullability Nullability { get; set; } = ProjectionNullability.unknown;

        [JsonPropertyName("upstream")]
        public ColumnReferenceFact[] Upstream { get; set; }
    }
}
