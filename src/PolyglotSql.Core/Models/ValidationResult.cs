using System.Text.Json;
using System.Text.Json.Serialization;

namespace PolyglotSql.Models
{
    public record ValidationResult
    {
        [JsonPropertyName("valid")]
        public bool Valid { get; set; }

        [JsonPropertyName("errors")]
        public ValidationError[] Errors { get; set; }
    }
}
