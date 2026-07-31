using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

using PolyglotSql.Models;

namespace PolyglotSql
{
    [JsonSerializable(typeof(string[]))]
    [JsonSerializable(typeof(Token[]))]
    [JsonSerializable(typeof(TokenType))]
    [JsonSerializable(typeof(DataType))]
    [JsonSerializable(typeof(StructField))]
    [JsonSerializable(typeof(UnionField))]
    [JsonSerializable(typeof(ObjectField))]
    [JsonSerializable(typeof(DiffResult))]
    [JsonSerializable(typeof(ValidationSchema))]
    [JsonSerializable(typeof(SchemaTable))]
    [JsonSerializable(typeof(SchemaColumn))]
    [JsonSerializable(typeof(QualifyTablesOptions))]
    [JsonSerializable(typeof(RenameTablesOptions))]
    [JsonSerializable(typeof(Dictionary<string, string>))]
    [JsonSerializable(typeof(OpenLineageOptions))]
    [JsonSerializable(typeof(OpenLineageDatasetId))]
    [JsonSerializable(typeof(Dictionary<string, OpenLineageDatasetId>))]
    [JsonSerializable(typeof(OpenLineageRunEventType))]
    [JsonSerializable(typeof(OpenLineageColumnLineageResult))]
    [JsonSerializable(typeof(ColumnLineageDatasetFacet))]
    [JsonSerializable(typeof(ColumnLineageField))]
    [JsonSerializable(typeof(OpenLineageInputField))]
    [JsonSerializable(typeof(OpenLineageTransformation))]
    [JsonSerializable(typeof(OpenLineageDataset))]
    [JsonSerializable(typeof(OpenLineageWarning))]
    [JsonSerializable(typeof(OpenLineageEventResult))]
    [JsonSerializable(typeof(Expression))]
    [JsonSerializable(typeof(Expression[]))]
    [JsonSerializable(typeof(AnalyzeQueryOptions))]
    [JsonSerializable(typeof(QueryAnalysis))]
    [JsonSerializable(typeof(ProjectionFact))]
    [JsonSerializable(typeof(ColumnReferenceFact))]
    [JsonSerializable(typeof(RelationFact))]
    [JsonSerializable(typeof(SetOperationFact))]
    [JsonSerializable(typeof(SetOperationBranchFact))]
    [JsonSerializable(typeof(ValidationResult))]
    [JsonSerializable(typeof(ValidationError))]
    [JsonSerializable(typeof(ValidationError[]))]
    [JsonSerializable(typeof(TranspileOptions))]
    [JsonSerializable(typeof(FormatGuardOptions))]
    public partial class PolyglotJsonContext : JsonSerializerContext
    {
    }
}
