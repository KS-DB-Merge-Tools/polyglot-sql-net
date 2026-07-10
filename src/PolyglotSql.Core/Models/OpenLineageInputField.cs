using System.Text.Json.Serialization;

namespace PolyglotSql.Models
{
    public record OpenLineageInputField
    {
        [JsonPropertyName("namespace")]
        public string Namespace { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("field")]
        public string Field { get; set; }

        [JsonPropertyName("transformations")]
        public OpenLineageTransformation[] Transformations { get; set; }
    }
}
