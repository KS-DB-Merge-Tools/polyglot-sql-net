using System.Text.Json;
using System.Text.Json.Serialization;

namespace PolyglotSql.Models
{
    [JsonConverter(typeof(JsonStringEnumConverter<ValidationSeverity>))]
    public enum ValidationSeverity
    {
        error,
        warning
    }

    public record ValidationError
    {
        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("line")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? Line { get; set; }

        [JsonPropertyName("column")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? Column { get; set; }

        [JsonPropertyName("severity")]
        public ValidationSeverity Severity { get; set; }

        [JsonPropertyName("code")]
        public string Code { get; set; }

        [JsonPropertyName("start")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? Start { get; set; }

        [JsonPropertyName("end")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? End { get; set; }
    }

    public record ValidationResult
    {
        [JsonPropertyName("valid")]
        public bool Valid { get; set; }

        [JsonPropertyName("errors")]
        public ValidationError[] Errors { get; set; }
    }
}
