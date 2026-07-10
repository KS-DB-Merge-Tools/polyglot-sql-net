using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PolyglotSql.Models
{
    public record ValidationSchema
    {
        [JsonPropertyName("tables")]
        public List<SchemaTable> Tables { get; set; } = new List<SchemaTable>();

        [JsonPropertyName("strict")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? Strict { get; set; }
    }
}
