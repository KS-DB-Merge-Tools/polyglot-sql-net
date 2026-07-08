using System.Text.Json.Serialization;

namespace PolyglotSql.Models
{
    public record FormatGuardOptions
    {
        [JsonPropertyName("maxInputBytes")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? MaxInputBytes { get; set; }

        [JsonPropertyName("maxTokens")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? MaxTokens { get; set; }

        [JsonPropertyName("maxAstNodes")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? MaxAstNodes { get; set; }

        [JsonPropertyName("maxSetOpChain")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? MaxSetOpChain { get; set; }
    }
}
