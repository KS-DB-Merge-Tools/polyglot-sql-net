using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PolyglotSql.Models
{
    public record SchemaForeignKey
    {
        [JsonPropertyName("name")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Name { get; set; }

        [JsonPropertyName("columns")]
        public List<string> Columns { get; set; } = new List<string>();

        [JsonPropertyName("references")]
        public SchemaTableReference References { get; set; }
    }
}
