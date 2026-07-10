using System.Text.Json;
using System.Text.Json.Serialization;

namespace PolyglotSql.Models
{
    public record OpenLineageEventResult
    {
        [JsonPropertyName("event")]
        public JsonElement Event { get; set; }

        [JsonPropertyName("warnings")]
        public OpenLineageWarning[] Warnings { get; set; }
    }
}
