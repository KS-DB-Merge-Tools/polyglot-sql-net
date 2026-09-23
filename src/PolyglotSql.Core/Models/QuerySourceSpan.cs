using System.Text.Json.Serialization;

namespace PolyglotSql.Models
{
    // A half-open range in the original SQL, measured in Unicode characters.
    public record QuerySourceSpan
    {
        [JsonPropertyName("start")]
        public int Start { get; set; }

        [JsonPropertyName("end")]
        public int End { get; set; }
    }
}
