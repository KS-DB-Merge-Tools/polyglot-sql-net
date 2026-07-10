using System.Text.Json.Serialization;

namespace PolyglotSql.Models
{
    [JsonConverter(typeof(JsonStringEnumConverter<ValidationSeverity>))]
    public enum ValidationSeverity
    {
        error,
        warning
    }
}
