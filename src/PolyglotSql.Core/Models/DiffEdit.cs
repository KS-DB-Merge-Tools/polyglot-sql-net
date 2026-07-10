using System.Text.Json.Serialization;

namespace PolyglotSql.Models
{
    public record DiffEdit
    {
        [JsonPropertyName("type")]
        public DiffEditType Type { get; set; } = DiffEditType.unknown;

        [JsonPropertyName("expression")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Expression Expression { get; set; }

        [JsonPropertyName("source")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Expression Source { get; set; }

        [JsonPropertyName("target")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Expression Target { get; set; }
    }
}
