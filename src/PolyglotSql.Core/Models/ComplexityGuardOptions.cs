using System.Text.Json.Serialization;

namespace PolyglotSql.Models
{
    // Per-call complexity limits for parse/transpile/validate/analyze.
    // A null property is omitted from JSON, so the native default for that limit applies.
    public record ComplexityGuardOptions
    {
        [JsonPropertyName("maxParserDepth")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public long? MaxParserDepth { get; set; }

        [JsonPropertyName("maxInputBytes")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public long? MaxInputBytes { get; set; }

        [JsonPropertyName("maxTokens")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public long? MaxTokens { get; set; }

        [JsonPropertyName("maxAstNodes")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public long? MaxAstNodes { get; set; }

        [JsonPropertyName("maxAstDepth")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public long? MaxAstDepth { get; set; }

        [JsonPropertyName("maxParenthesisDepth")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public long? MaxParenthesisDepth { get; set; }

        [JsonPropertyName("maxFunctionCallDepth")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public long? MaxFunctionCallDepth { get; set; }
    }
}
