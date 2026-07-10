using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PolyglotSql.Models
{
    public record ColumnLineageDatasetFacet
    {
        [JsonPropertyName("_producer")]
        public string Producer { get; set; }

        [JsonPropertyName("_schemaURL")]
        public string SchemaUrl { get; set; }

        [JsonPropertyName("fields")]
        public Dictionary<string, ColumnLineageField> Fields { get; set; }
    }
}
