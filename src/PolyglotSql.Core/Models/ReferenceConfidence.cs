using System.Text.Json.Serialization;

namespace PolyglotSql.Models
{
    [JsonConverter(typeof(JsonStringEnumConverter<ReferenceConfidence>))]
    public enum ReferenceConfidence
    {
        resolved,
        ambiguous,
        unknown
    }
}
