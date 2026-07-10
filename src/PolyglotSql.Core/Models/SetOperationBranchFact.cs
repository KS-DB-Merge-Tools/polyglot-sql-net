using System.Text.Json.Serialization;

namespace PolyglotSql.Models
{
    public record SetOperationBranchFact
    {
        [JsonPropertyName("index")]
        public int Index { get; set; }

        [JsonPropertyName("projections")]
        public ProjectionFact[] Projections { get; set; }
    }
}
