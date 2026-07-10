using System.Text.Json.Serialization;

namespace PolyglotSql.Models
{
    [JsonConverter(typeof(JsonStringEnumConverter<OpenLineageRunEventType>))]
    public enum OpenLineageRunEventType
    {
        START,
        RUNNING,
        COMPLETE,
        ABORT,
        FAIL,
        OTHER
    }
}
