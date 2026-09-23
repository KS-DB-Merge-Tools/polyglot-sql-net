using System.Text.Json.Serialization;

namespace PolyglotSql.Models
{
    public record SchemaColumnReference
    {
        [JsonPropertyName("table")]
        public string Table { get; set; }

        [JsonPropertyName("column")]
        public string Column { get; set; }

        [JsonPropertyName("schema")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Schema { get; set; }
    }
}
