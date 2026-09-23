using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PolyglotSql.Models
{
    // The ordered output description of a query.
    public record QueryOutput
    {
        [JsonPropertyName("columns")]
        public List<OutputColumn> Columns { get; set; } = new List<OutputColumn>();

        // Whether every entry has a stable zero-based ordinal.
        [JsonPropertyName("ordinalComplete")]
        public bool OrdinalComplete { get; set; }
    }
}
