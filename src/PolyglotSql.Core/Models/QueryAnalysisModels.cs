using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PolyglotSql.Models
{
    [JsonConverter(typeof(JsonStringEnumConverter<QueryShape>))]
    public enum QueryShape
    {
        select,
        set_operation
    }

    [JsonConverter(typeof(JsonStringEnumConverter<TransformKind>))]
    public enum TransformKind
    {
        direct,
        cast,
        aggregation,
        constant,
        expression,
        star
    }

    [JsonConverter(typeof(JsonStringEnumConverter<ReferenceConfidence>))]
    public enum ReferenceConfidence
    {
        resolved,
        ambiguous,
        unknown
    }

    public record AnalyzeQueryOptions
    {
        [JsonPropertyName("dialect")]
        [JsonConverter(typeof(DialectJsonConverter))]
        public Dialect Dialect { get; set; } = Dialect.Generic;

        [JsonPropertyName("schema")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ValidationSchema Schema { get; set; }
    }

    public record QueryAnalysis
    {
        [JsonPropertyName("shape")]
        public QueryShape Shape { get; set; }

        [JsonPropertyName("ctes")]
        public string[] Ctes { get; set; }

        [JsonPropertyName("projections")]
        public ProjectionFact[] Projections { get; set; }

        [JsonPropertyName("relations")]
        public RelationFact[] Relations { get; set; }

        [JsonPropertyName("setOperations")]
        public SetOperationFact[] SetOperations { get; set; }
    }

    public record ProjectionFact
    {
        [JsonPropertyName("index")]
        public int Index { get; set; }

        [JsonPropertyName("name")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Name { get; set; }

        [JsonPropertyName("isStar")]
        public bool IsStar { get; set; }

        [JsonPropertyName("starTable")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string StarTable { get; set; }

        [JsonPropertyName("transformKind")]
        public TransformKind TransformKind { get; set; }

        [JsonPropertyName("castType")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string CastType { get; set; }

        [JsonPropertyName("typeHint")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string TypeHint { get; set; }

        [JsonPropertyName("upstream")]
        public ColumnReferenceFact[] Upstream { get; set; }
    }

    public record ColumnReferenceFact
    {
        [JsonPropertyName("sourceName")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string SourceName { get; set; }

        [JsonPropertyName("sourceAlias")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string SourceAlias { get; set; }

        [JsonPropertyName("sourceKind")]
        public SourceKind SourceKind { get; set; }

        [JsonPropertyName("table")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Table { get; set; }

        [JsonPropertyName("column")]
        public string Column { get; set; }

        [JsonPropertyName("unqualified")]
        public bool Unqualified { get; set; }

        [JsonPropertyName("confidence")]
        public ReferenceConfidence Confidence { get; set; }
    }

    public record RelationFact
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("alias")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Alias { get; set; }

        [JsonPropertyName("kind")]
        public SourceKind Kind { get; set; }

        [JsonPropertyName("columns")]
        public string[] Columns { get; set; }
    }

    public record SetOperationFact
    {
        [JsonPropertyName("kind")]
        public string Kind { get; set; }

        [JsonPropertyName("all")]
        public bool All { get; set; }

        [JsonPropertyName("distinct")]
        public bool Distinct { get; set; }

        [JsonPropertyName("outputColumns")]
        public string[] OutputColumns { get; set; }

        [JsonPropertyName("branches")]
        public SetOperationBranchFact[] Branches { get; set; }
    }

    public record SetOperationBranchFact
    {
        [JsonPropertyName("index")]
        public int Index { get; set; }

        [JsonPropertyName("projections")]
        public ProjectionFact[] Projections { get; set; }
    }
}
