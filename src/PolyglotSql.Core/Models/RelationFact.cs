using System.Text.Json.Serialization;

namespace PolyglotSql.Models
{
    public record RelationFact
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("alias")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Alias { get; set; }

        [JsonPropertyName("kind")]
        public SourceKind Kind { get; set; }

        [JsonPropertyName("columns")]
        public string[] Columns { get; set; }

        [JsonPropertyName("catalog")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Catalog { get; set; }

        [JsonPropertyName("schema")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Schema { get; set; }

        [JsonPropertyName("table")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Table { get; set; }
    }
}
