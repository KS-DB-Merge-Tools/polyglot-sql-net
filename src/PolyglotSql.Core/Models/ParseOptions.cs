using System.Text.Json.Serialization;

namespace PolyglotSql.Models
{
    public record ParseOptions
    {
        [JsonPropertyName("complexityGuard")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ComplexityGuardOptions ComplexityGuard { get; set; }
    }
}
