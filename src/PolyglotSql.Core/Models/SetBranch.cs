using System.Text.Json.Serialization;

namespace PolyglotSql.Models
{
    // Metadata describing a lineage node's position in an immediate set operation.
    public record SetBranch
    {
        [JsonPropertyName("operator")]
        public SetOperator Operator { get; set; }

        // Zero-based branch ordinal within the immediate binary operation.
        [JsonPropertyName("ordinal")]
        public int Ordinal { get; set; }

        // Whether the immediate set operation uses ALL semantics.
        [JsonPropertyName("all")]
        public bool All { get; set; }
    }
}
