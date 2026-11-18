using System.Text.Json.Serialization;

namespace PolyglotSql.Models
{
    [JsonConverter(typeof(JsonStringEnumConverter<UnsupportedLevel>))]
    public enum UnsupportedLevel
    {
        warn = 0,
        ignore = 1,
        raise = 2,
        immediate = 3,
    }
}
