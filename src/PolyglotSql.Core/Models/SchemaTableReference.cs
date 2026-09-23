using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PolyglotSql.Models
{
    public record SchemaTableReference
    {
        [JsonPropertyName("table")]
        public string Table { get; set; }

        [JsonPropertyName("columns")]
        public List<string> Columns { get; set; } = new List<string>();

        [JsonPropertyName("schema")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Schema { get; set; }
    }
}
