using System.Text.Json.Serialization;

namespace PolyglotSql.Models
{
    // Compact fact about one top-level CTE definition.
    public record CteFact
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("columns")]
        public string[] Columns { get; set; }

        [JsonPropertyName("bodySql")]
        public string BodySql { get; set; }

        [JsonPropertyName("outputColumns")]
        public string[] OutputColumns { get; set; }
    }
}
