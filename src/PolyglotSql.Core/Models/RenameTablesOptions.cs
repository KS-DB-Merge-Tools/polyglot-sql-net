using System.Text.Json.Serialization;

namespace PolyglotSql.Models
{
    public record RenameTablesOptions
    {
        [JsonPropertyName("aliasRenamedTables")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? AliasRenamedTables { get; set; }

        [JsonPropertyName("preserveExistingAliases")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? PreserveExistingAliases { get; set; }
    }
}
