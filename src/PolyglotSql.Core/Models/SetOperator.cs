using System.Text.Json.Serialization;

namespace PolyglotSql.Models
{
    // The set operator that owns an immediate lineage branch.
    [JsonConverter(typeof(JsonStringEnumConverter<SetOperator>))]
    public enum SetOperator
    {
        union,
        intersect,
        except
    }
}
