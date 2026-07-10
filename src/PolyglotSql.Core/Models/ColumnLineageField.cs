using System.Text.Json.Serialization;

namespace PolyglotSql.Models
{
    public record ColumnLineageField
    {
        [JsonPropertyName("inputFields")]
        public OpenLineageInputField[] InputFields { get; set; }
    }
}
