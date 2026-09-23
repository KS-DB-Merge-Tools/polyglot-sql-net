using System.Text.Json.Serialization;

namespace PolyglotSql.Models
{
    // Conservative nullability classification for one output projection.
    [JsonConverter(typeof(JsonStringEnumConverter<ProjectionNullability>))]
    public enum ProjectionNullability
    {
        non_null,
        nullable,
        unknown
    }
}
