using System.Text.Json.Serialization;

namespace PolyglotSql.Models
{
    public record QueryAnalysis
    {
        [JsonPropertyName("shape")]
        public QueryShape Shape { get; set; }

        [JsonPropertyName("ctes")]
        public string[] Ctes { get; set; }

        [JsonPropertyName("cteFacts")]
        public CteFact[] CteFacts { get; set; }

        [JsonPropertyName("projections")]
        public ProjectionFact[] Projections { get; set; }

        [JsonPropertyName("relations")]
        public RelationFact[] Relations { get; set; }

        [JsonPropertyName("baseTables")]
        public RelationFact[] BaseTables { get; set; }

        [JsonPropertyName("starProjections")]
        public StarProjectionFact[] StarProjections { get; set; }

        [JsonPropertyName("setOperations")]
        public SetOperationFact[] SetOperations { get; set; }

        // Clause-specific column uses, separate from output projection lineage.
        [JsonPropertyName("columnUses")]
        public ColumnUseFact[] ColumnUses { get; set; }
    }
}
