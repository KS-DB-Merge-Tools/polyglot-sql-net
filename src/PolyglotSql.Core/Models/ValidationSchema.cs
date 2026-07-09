using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PolyglotSql.Models
{
    public record SchemaColumn
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("type")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Type { get; set; }

        [JsonPropertyName("nullable")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? Nullable { get; set; }

        [JsonPropertyName("primaryKey")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public bool PrimaryKey { get; set; }

        [JsonPropertyName("unique")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public bool Unique { get; set; }
    }

    public record SchemaTable
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("schema")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Schema { get; set; }

        [JsonPropertyName("columns")]
        public List<SchemaColumn> Columns { get; set; } = new List<SchemaColumn>();

        [JsonPropertyName("aliases")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<string> Aliases { get; set; }

        [JsonPropertyName("primaryKey")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<string> PrimaryKey { get; set; }

        [JsonPropertyName("uniqueKeys")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<List<string>> UniqueKeys { get; set; }
    }

    public record ValidationSchema
    {
        [JsonPropertyName("tables")]
        public List<SchemaTable> Tables { get; set; } = new List<SchemaTable>();

        [JsonPropertyName("strict")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? Strict { get; set; }
    }
}
