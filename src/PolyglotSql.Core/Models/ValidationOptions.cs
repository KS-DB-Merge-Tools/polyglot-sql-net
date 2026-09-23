using System.Text.Json.Serialization;

namespace PolyglotSql.Models
{
    public record ValidationOptions
    {
        [JsonPropertyName("complexityGuard")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ComplexityGuardOptions ComplexityGuard { get; set; }

        [JsonPropertyName("strictSyntax")]
        public bool StrictSyntax { get; set; }

        [JsonPropertyName("semantic")]
        public bool Semantic { get; set; }
    }
}
