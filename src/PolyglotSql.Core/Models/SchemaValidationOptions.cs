using System.Text.Json.Serialization;

namespace PolyglotSql.Models
{
    // Native SchemaValidationOptions uses snake_case names (camelCase is accepted only as aliases).
    public record SchemaValidationOptions
    {
        [JsonPropertyName("complexity_guard")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ComplexityGuardOptions ComplexityGuard { get; set; }

        [JsonPropertyName("check_types")]
        public bool CheckTypes { get; set; }

        [JsonPropertyName("check_references")]
        public bool CheckReferences { get; set; }

        // If set, overrides ValidationSchema.Strict.
        [JsonPropertyName("strict")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? Strict { get; set; }

        [JsonPropertyName("semantic")]
        public bool Semantic { get; set; }

        [JsonPropertyName("strict_syntax")]
        public bool StrictSyntax { get; set; }
    }
}
