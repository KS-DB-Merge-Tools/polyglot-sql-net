using System.Text.Json.Serialization;

namespace PolyglotSql.Models
{
    // Semantic contribution of one set-operation branch.
    [JsonConverter(typeof(JsonStringEnumConverter<SetOperationBranchRole>))]
    public enum SetOperationBranchRole
    {
        value,
        filter
    }
}
