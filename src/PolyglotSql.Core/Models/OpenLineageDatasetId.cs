using System.Text.Json.Serialization;

namespace PolyglotSql.Models
{
    public record OpenLineageDatasetId
    {
        [JsonPropertyName("namespace")]
        public string Namespace { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }
    }
}
