using System.Text.Json.Serialization;

namespace PolyglotSql.Models
{
    [JsonConverter(typeof(JsonStringEnumConverter<QueryShape>))]
    public enum QueryShape
    {
        select,
        set_operation
    }
}
