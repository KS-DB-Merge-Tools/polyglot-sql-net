using System.Text.Json.Serialization;

namespace PolyglotSql.Models
{
    public record OpenLineageWarning
    {
        [JsonPropertyName("code")]
        public string Code { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }
    }
}
