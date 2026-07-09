using System.Collections.Generic;
using System.Text.Json;
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

    public record ColumnLineageDatasetFacet
    {
        [JsonPropertyName("_producer")]
        public string Producer { get; set; }

        [JsonPropertyName("_schemaURL")]
        public string SchemaUrl { get; set; }

        [JsonPropertyName("fields")]
        public Dictionary<string, ColumnLineageField> Fields { get; set; }
    }

    public record ColumnLineageField
    {
        [JsonPropertyName("inputFields")]
        public OpenLineageInputField[] InputFields { get; set; }
    }

    public record OpenLineageInputField
    {
        [JsonPropertyName("namespace")]
        public string Namespace { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("field")]
        public string Field { get; set; }

        [JsonPropertyName("transformations")]
        public OpenLineageTransformation[] Transformations { get; set; }
    }

    public record OpenLineageTransformation
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("subtype")]
        public string Subtype { get; set; }

        [JsonPropertyName("description")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Description { get; set; }

        [JsonPropertyName("masking")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? Masking { get; set; }
    }

    public record OpenLineageDataset
    {
        [JsonPropertyName("namespace")]
        public string Namespace { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("facets")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Dictionary<string, JsonElement> Facets { get; set; }
    }

    public record OpenLineageWarning
    {
        [JsonPropertyName("code")]
        public string Code { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }
    }

    public record OpenLineageEventResult
    {
        [JsonPropertyName("event")]
        public JsonElement Event { get; set; }

        [JsonPropertyName("warnings")]
        public OpenLineageWarning[] Warnings { get; set; }
    }
}
