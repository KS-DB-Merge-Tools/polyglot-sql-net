using System.Text.Json.Serialization;

namespace PolyglotSql.Models
{
    public record QueryAnalysis
    {
        [JsonPropertyName("shape")]
        public QueryShape Shape { get; set; }

        [JsonPropertyName("ctes")]
        public string[] Ctes { get; set; }

        [JsonPropertyName("projections")]
        public ProjectionFact[] Projections { get; set; }

        [JsonPropertyName("relations")]
        public RelationFact[] Relations { get; set; }

        [JsonPropertyName("setOperations")]
        public SetOperationFact[] SetOperations { get; set; }
    }
}
