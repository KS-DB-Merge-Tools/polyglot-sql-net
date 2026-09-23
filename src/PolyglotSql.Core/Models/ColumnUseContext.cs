using System.Text.Json.Serialization;

namespace PolyglotSql.Models
{
    // The syntactic role of a column-containing expression.
    [JsonConverter(typeof(JsonStringEnumConverter<ColumnUseContext>))]
    public enum ColumnUseContext
    {
        join,
        filter,
        group,
        having,
        qualify,
        window_partition,
        window_order,
        window_frame,
        order,
        aggregate_order,
        set_operation_filter
    }
}
