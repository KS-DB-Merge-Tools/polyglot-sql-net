using System.Text.Json.Serialization;

namespace PolyglotSql.Models
{
    public record SetOperationFact
    {
        [JsonPropertyName("kind")]
        public string Kind { get; set; }

        [JsonPropertyName("all")]
        public bool All { get; set; }

        [JsonPropertyName("distinct")]
        public bool Distinct { get; set; }

        [JsonPropertyName("outputColumns")]
        public string[] OutputColumns { get; set; }

        [JsonPropertyName("branches")]
        public SetOperationBranchFact[] Branches { get; set; }
    }
}
