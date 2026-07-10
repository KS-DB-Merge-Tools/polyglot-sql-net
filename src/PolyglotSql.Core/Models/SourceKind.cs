using System.Text.Json.Serialization;

namespace PolyglotSql.Models
{
    [JsonConverter(typeof(JsonStringEnumConverter<SourceKind>))]
    public enum SourceKind
    {
        unknown,
        root,
        cte,
        derived_table,
        subquery,
        table,
        @virtual,
        view,
        lateral,
        unnest
    }
}
