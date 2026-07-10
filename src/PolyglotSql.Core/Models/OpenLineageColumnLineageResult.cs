using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PolyglotSql.Models
{
    public record OpenLineageColumnLineageResult
    {
        [JsonPropertyName("facet")]
        public ColumnLineageDatasetFacet Facet { get; set; }

        [JsonPropertyName("inputs")]
        public OpenLineageDataset[] Inputs { get; set; }

        [JsonPropertyName("outputs")]
        public OpenLineageDataset[] Outputs { get; set; }

        [JsonPropertyName("warnings")]
        public OpenLineageWarning[] Warnings { get; set; }
    }
}
