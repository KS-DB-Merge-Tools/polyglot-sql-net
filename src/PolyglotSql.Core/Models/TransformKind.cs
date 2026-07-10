using System.Text.Json.Serialization;

namespace PolyglotSql.Models
{
    [JsonConverter(typeof(JsonStringEnumConverter<TransformKind>))]
    public enum TransformKind
    {
        direct,
        cast,
        aggregation,
        constant,
        expression,
        star
    }
}
