using System.Text.Json.Serialization;

namespace PolyglotSql.Models
{
    public record TranspileOptions
    {
        [JsonPropertyName("pretty")]
        public bool Pretty { get; set; }

        [JsonPropertyName("unsupportedLevel")]
        public UnsupportedLevel UnsupportedLevel { get; set; } = UnsupportedLevel.warn;

        [JsonPropertyName("maxUnsupported")]
        public int MaxUnsupported { get; set; }

        [JsonPropertyName("complexityGuard")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ComplexityGuardOptions ComplexityGuard { get; set; }
    }
}
