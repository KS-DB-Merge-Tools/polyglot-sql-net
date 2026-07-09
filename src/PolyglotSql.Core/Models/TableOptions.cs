using System.Text.Json.Serialization;

namespace PolyglotSql.Models
{
    public record QualifyTablesOptions
    {
        [JsonPropertyName("db")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Db { get; set; }

        [JsonPropertyName("catalog")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Catalog { get; set; }

        [JsonPropertyName("dialect")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Dialect { get; set; }

        [JsonPropertyName("canonicalizeTableAliases")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? CanonicalizeTableAliases { get; set; }

        [JsonPropertyName("aliasUnaliasedTables")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? AliasUnaliasedTables { get; set; }

        [JsonPropertyName("aliasUnaliasedSubqueries")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? AliasUnaliasedSubqueries { get; set; }

        [JsonPropertyName("aliasPrefix")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string AliasPrefix { get; set; }

        [JsonPropertyName("normalizeSetOperationSubqueries")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? NormalizeSetOperationSubqueries { get; set; }
    }

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
