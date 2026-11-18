using System.Text.Json;
using PolyglotSql;
using PolyglotSql.Models;

namespace PolyglotSql.Tests;

// Pure serialization tests - no native library required
public class QueryAnalysisModelTests
{
    private const string AnalysisJson = @"{
        ""shape"": ""select"",
        ""ctes"": [""c""],
        ""cteFacts"": [ { ""name"": ""c"", ""columns"": [""x""], ""bodySql"": ""SELECT a FROM t"", ""outputColumns"": [""a""] } ],
        ""projections"": [ {
            ""index"": 0, ""name"": ""m"", ""isStar"": false, ""starTable"": null, ""transformKind"": ""expression"",
            ""transformFunction"": { ""name"": ""DATE_TRUNC"", ""literalArgs"": [""MONTH""], ""columnArgs"": [
                { ""sourceName"": ""t"", ""sourceAlias"": null, ""sourceKind"": ""table"", ""table"": ""t"", ""column"": ""created_at"", ""unqualified"": true, ""confidence"": ""resolved"" } ] },
            ""castType"": null, ""typeHint"": null, ""nullability"": ""non_null"", ""upstream"": [
                { ""sourceName"": ""t"", ""sourceAlias"": null, ""sourceKind"": ""table"", ""table"": ""t"", ""column"": ""amount"", ""unqualified"": true, ""confidence"": ""resolved"",
                  ""pathTransformKind"": ""aggregation"", ""pathFunctions"": [""ROUND"", ""SUM""], ""pathWindow"": true } ]
        } ],
        ""relations"": [ { ""name"": ""t"", ""alias"": null, ""kind"": ""table"", ""columns"": [], ""catalog"": ""db"", ""schema"": ""sch"", ""table"": ""t"" } ],
        ""baseTables"": [ { ""name"": ""t"", ""alias"": null, ""kind"": ""table"", ""columns"": [], ""catalog"": null, ""schema"": null, ""table"": ""t"" } ],
        ""starProjections"": [ { ""index"": 1, ""table"": ""t"", ""expandedColumns"": [""a"", ""b""] } ],
        ""setOperations"": [ { ""kind"": ""union"", ""all"": true, ""distinct"": false, ""outputColumns"": [""a""], ""branches"": [
            { ""index"": 0, ""role"": ""value"", ""projections"": [] },
            { ""index"": 1, ""role"": ""filter"", ""projections"": [] } ] } ],
        ""columnUses"": [ {
            ""context"": ""filter"", ""scopePath"": ""root"", ""expressionPath"": ""where_clause.this"", ""expressionSql"": ""o.amount > 10"",
            ""span"": { ""start"": 29, ""end"": 42 },
            ""references"": [ { ""sourceName"": ""orders"", ""sourceAlias"": ""o"", ""sourceKind"": ""table"", ""table"": ""o"",
                ""column"": ""amount"", ""unqualified"": false, ""confidence"": ""resolved"", ""span"": { ""start"": 35, ""end"": 43 } } ]
        } ]
    }";

    private static QueryAnalysis Deserialize() => JsonSerializer.Deserialize(AnalysisJson, PolyglotJsonContext.Default.QueryAnalysis)!;

    [Fact]
    public void TestCteFacts()
    {
        var cte = Assert.Single(Deserialize().CteFacts);

        Assert.Equal("c", cte.Name);
        Assert.Equal(new[] { "x" }, cte.Columns);
        Assert.Equal("SELECT a FROM t", cte.BodySql);
        Assert.Equal(new[] { "a" }, cte.OutputColumns);
    }

    [Fact]
    public void TestBaseTables()
    {
        var table = Assert.Single(Deserialize().BaseTables);

        Assert.Equal("t", table.Name);
        Assert.Equal(SourceKind.table, table.Kind);
    }

    [Fact]
    public void TestRelationCatalogSchemaTable()
    {
        var relation = Assert.Single(Deserialize().Relations);

        Assert.Equal("db", relation.Catalog);
        Assert.Equal("sch", relation.Schema);
        Assert.Equal("t", relation.Table);
    }

    [Fact]
    public void TestStarProjections()
    {
        var star = Assert.Single(Deserialize().StarProjections);

        Assert.Equal(1, star.Index);
        Assert.Equal("t", star.Table);
        Assert.Equal(new[] { "a", "b" }, star.ExpandedColumns);
    }

    [Fact]
    public void TestProjectionTransformFunctionAndNullability()
    {
        var projection = Assert.Single(Deserialize().Projections);

        Assert.Equal(ProjectionNullability.non_null, projection.Nullability);
        Assert.Equal("DATE_TRUNC", projection.TransformFunction.Name);
        Assert.Equal(new[] { "MONTH" }, projection.TransformFunction.LiteralArgs);
        Assert.Equal("created_at", Assert.Single(projection.TransformFunction.ColumnArgs).Column);
    }

    [Fact]
    public void TestSetOperationBranchRole()
    {
        var branches = Assert.Single(Deserialize().SetOperations).Branches;

        Assert.Equal(SetOperationBranchRole.value, branches[0].Role);
        Assert.Equal(SetOperationBranchRole.filter, branches[1].Role);
    }

    [Fact]
    public void TestColumnUses()
    {
        var use = Assert.Single(Deserialize().ColumnUses);

        Assert.Equal(ColumnUseContext.filter, use.Context);
        Assert.Equal("root", use.ScopePath);
        Assert.Equal("where_clause.this", use.ExpressionPath);
        Assert.Equal("o.amount > 10", use.ExpressionSql);
        Assert.Equal(29, use.Span.Start);
        Assert.Equal(42, use.Span.End);

        // ColumnReferenceFact fields are flattened into the reference object
        var reference = Assert.Single(use.References);
        Assert.Equal("orders", reference.SourceName);
        Assert.Equal("o", reference.SourceAlias);
        Assert.Equal(SourceKind.table, reference.SourceKind);
        Assert.Equal("o", reference.Table);
        Assert.Equal("amount", reference.Column);
        Assert.False(reference.Unqualified);
        Assert.Equal(ReferenceConfidence.resolved, reference.Confidence);
        Assert.Equal(35, reference.Span.Start);
        Assert.Equal(43, reference.Span.End);
    }

    [Fact]
    public void TestLegacyJsonWithoutNewFields()
    {
        // Older native versions do not emit the new fields
        string json = @"{ ""shape"": ""select"", ""ctes"": [], ""projections"": [ { ""index"": 0, ""name"": ""a"", ""isStar"": false,
            ""transformKind"": ""direct"", ""upstream"": [] } ], ""relations"": [], ""setOperations"": [] }";
        var analysis = JsonSerializer.Deserialize(json, PolyglotJsonContext.Default.QueryAnalysis)!;

        Assert.Null(analysis.ColumnUses);
        Assert.Null(analysis.CteFacts);
        Assert.Equal(ProjectionNullability.unknown, analysis.Projections[0].Nullability);
        Assert.Null(analysis.Projections[0].TransformFunction);
    }

    [Fact]
    public void TestLegacyUpstreamWithoutPathFields()
    {
        // Native before v0.13.5 does not emit the path fields; newer native omits them when unset
        string json = @"{ ""sourceName"": ""t"", ""sourceKind"": ""table"", ""column"": ""a"", ""unqualified"": true, ""confidence"": ""resolved"" }";
        var reference = JsonSerializer.Deserialize(json, PolyglotJsonContext.Default.ColumnReferenceFact)!;

        Assert.Null(reference.PathTransformKind);
        Assert.NotNull(reference.PathFunctions);
        Assert.Empty(reference.PathFunctions);
        Assert.False(reference.PathWindow);
    }

    [Fact]
    public void TestProjectionUpstreamPathFields()
    {
        var upstream = Assert.Single(Assert.Single(Deserialize().Projections).Upstream);

        Assert.Equal("amount", upstream.Column);
        Assert.Equal(TransformKind.aggregation, upstream.PathTransformKind);
        Assert.Equal(new[] { "ROUND", "SUM" }, upstream.PathFunctions);
        Assert.True(upstream.PathWindow);
    }

    [Fact]
    public void TestColumnUseReferencePathFieldsAbsent()
    {
        var reference = Assert.Single(Assert.Single(Deserialize().ColumnUses).References);

        Assert.Null(reference.PathTransformKind);
        Assert.NotNull(reference.PathFunctions);
        Assert.Empty(reference.PathFunctions);
        Assert.False(reference.PathWindow);
    }

    [Fact]
    public void TestColumnReferenceFactPathFieldsRoundTrip()
    {
        string json = @"{""sourceName"":""t"",""sourceKind"":""table"",""table"":""t"",""column"":""amount"",""unqualified"":false,""confidence"":""resolved""," +
                      @"""pathTransformKind"":""cast"",""pathFunctions"":[""ROW_NUMBER""],""pathWindow"":true}";
        var reference = JsonSerializer.Deserialize(json, PolyglotJsonContext.Default.ColumnReferenceFact)!;

        Assert.Equal(TransformKind.cast, reference.PathTransformKind);
        Assert.Equal(new[] { "ROW_NUMBER" }, reference.PathFunctions);
        Assert.True(reference.PathWindow);
        Assert.Equal(json, JsonSerializer.Serialize(reference, PolyglotJsonContext.Default.ColumnReferenceFact));
    }

    [Fact]
    public void TestColumnReferenceFactPathFieldsDefaultsSerialization()
    {
        var reference = new ColumnReferenceFact { SourceKind = SourceKind.table, Column = "a", Confidence = ReferenceConfidence.resolved };
        string json = JsonSerializer.Serialize(reference, PolyglotJsonContext.Default.ColumnReferenceFact);

        Assert.DoesNotContain("pathTransformKind", json);
        Assert.DoesNotContain("pathWindow", json);
        Assert.Contains(@"""pathFunctions"":[]", json);
    }

    [Fact]
    public void TestColumnUseReferenceFactPathFieldsRoundTrip()
    {
        var reference = new ColumnUseReferenceFact
        {
            SourceName = "orders",
            SourceKind = SourceKind.table,
            Column = "amount",
            Confidence = ReferenceConfidence.resolved,
            PathTransformKind = TransformKind.expression,
            PathFunctions = new[] { "ABS" },
            PathWindow = true,
            Span = new QuerySourceSpan { Start = 1, End = 7 }
        };
        var analysis = new QueryAnalysis { ColumnUses = new[] { new ColumnUseFact { Context = ColumnUseContext.filter, References = new[] { reference } } } };

        string json = JsonSerializer.Serialize(analysis, PolyglotJsonContext.Default.QueryAnalysis);
        Assert.Contains(@"""pathTransformKind"":""expression"",""pathFunctions"":[""ABS""],""pathWindow"":true", json);

        var parsed = Assert.Single(Assert.Single(JsonSerializer.Deserialize(json, PolyglotJsonContext.Default.QueryAnalysis)!.ColumnUses).References);
        Assert.Equal(TransformKind.expression, parsed.PathTransformKind);
        Assert.Equal(new[] { "ABS" }, parsed.PathFunctions);
        Assert.True(parsed.PathWindow);
        Assert.Equal(1, parsed.Span.Start);
        Assert.Equal(7, parsed.Span.End);
    }

    [Theory]
    [InlineData("direct", TransformKind.direct)]
    [InlineData("cast", TransformKind.cast)]
    [InlineData("aggregation", TransformKind.aggregation)]
    [InlineData("constant", TransformKind.constant)]
    [InlineData("expression", TransformKind.expression)]
    [InlineData("star", TransformKind.star)]
    public void TestPathTransformKindValues(string json, TransformKind expected)
    {
        string referenceJson = $@"{{""sourceKind"":""table"",""column"":""a"",""unqualified"":false,""confidence"":""resolved"",""pathTransformKind"":""{json}""}}";
        var reference = JsonSerializer.Deserialize(referenceJson, PolyglotJsonContext.Default.ColumnReferenceFact)!;

        Assert.Equal(expected, reference.PathTransformKind);
        Assert.Contains($@"""pathTransformKind"":""{json}""", JsonSerializer.Serialize(reference, PolyglotJsonContext.Default.ColumnReferenceFact));
    }

    [Theory]
    [InlineData("join", ColumnUseContext.join)]
    [InlineData("filter", ColumnUseContext.filter)]
    [InlineData("group", ColumnUseContext.group)]
    [InlineData("having", ColumnUseContext.having)]
    [InlineData("qualify", ColumnUseContext.qualify)]
    [InlineData("window_partition", ColumnUseContext.window_partition)]
    [InlineData("window_order", ColumnUseContext.window_order)]
    [InlineData("window_frame", ColumnUseContext.window_frame)]
    [InlineData("order", ColumnUseContext.order)]
    [InlineData("aggregate_order", ColumnUseContext.aggregate_order)]
    [InlineData("set_operation_filter", ColumnUseContext.set_operation_filter)]
    public void TestColumnUseContextValues(string json, ColumnUseContext expected)
    {
        Assert.Equal(expected, JsonSerializer.Deserialize($"\"{json}\"", PolyglotJsonContext.Default.ColumnUseContext));
    }

    [Theory]
    [InlineData("non_null", ProjectionNullability.non_null)]
    [InlineData("nullable", ProjectionNullability.nullable)]
    [InlineData("unknown", ProjectionNullability.unknown)]
    public void TestProjectionNullabilityValues(string json, ProjectionNullability expected)
    {
        Assert.Equal(expected, JsonSerializer.Deserialize($"\"{json}\"", PolyglotJsonContext.Default.ProjectionNullability));
    }
}
