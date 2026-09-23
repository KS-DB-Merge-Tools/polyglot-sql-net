using System.Collections.Generic;
using PolyglotSql;
using PolyglotSql.Bundle;
using PolyglotSql.Models;

namespace PolyglotSql.Tests;

public class AnalyzeQueryResultTests
{
    static AnalyzeQueryResultTests()
    {
        BundleInitializer.Initialize();
    }

    [Fact]
    public void TestCteFactsAndBaseTables()
    {
        var analysis = Polyglot.AnalyzeQuery("WITH c AS (SELECT a FROM t) SELECT a FROM c");

        var cte = Assert.Single(analysis.CteFacts);
        Assert.Equal("c", cte.Name);
        Assert.Contains("SELECT a FROM t", cte.BodySql);
        Assert.Contains(analysis.BaseTables, r => r.Name == "t");
    }

    [Fact]
    public void TestRelationCatalogSchemaTable()
    {
        var analysis = Polyglot.AnalyzeQuery("SELECT a FROM db.sch.t");

        var relation = Assert.Single(analysis.Relations);
        Assert.Equal("db", relation.Catalog);
        Assert.Equal("sch", relation.Schema);
        Assert.Equal("t", relation.Table);
    }

    [Fact]
    public void TestStarProjectionsWithSchema()
    {
        var options = new AnalyzeQueryOptions
        {
            Schema = new ValidationSchema
            {
                Tables =
                {
                    new SchemaTable
                    {
                        Name = "t",
                        Columns = { new SchemaColumn { Name = "a", Type = "int" }, new SchemaColumn { Name = "b", Type = "int" } }
                    }
                }
            }
        };
        var analysis = Polyglot.AnalyzeQuery("SELECT * FROM t", options);

        var star = Assert.Single(analysis.StarProjections);
        Assert.Equal(0, star.Index);
        Assert.Equal(new[] { "a", "b" }, star.ExpandedColumns);
    }

    [Fact]
    public void TestProjectionNullability()
    {
        var options = new AnalyzeQueryOptions
        {
            Schema = new ValidationSchema
            {
                Tables = { new SchemaTable { Name = "t", Columns = { new SchemaColumn { Name = "a", Type = "int", Nullable = false } } } }
            }
        };
        var analysis = Polyglot.AnalyzeQuery("SELECT a FROM t", options);

        Assert.Equal(ProjectionNullability.non_null, Assert.Single(analysis.Projections).Nullability);
    }

    [Fact]
    public void TestProjectionTransformFunction()
    {
        var options = new AnalyzeQueryOptions { Dialect = Dialect.PostgreSQL };
        var analysis = Polyglot.AnalyzeQuery("SELECT DATE_TRUNC('month', created_at) AS m FROM t", options);

        var function = Assert.Single(analysis.Projections).TransformFunction;
        Assert.NotNull(function);
        Assert.Equal("DATE_TRUNC", function.Name);
        Assert.Contains(function.LiteralArgs, a => a.Equals("month", StringComparison.OrdinalIgnoreCase));
        Assert.Equal("created_at", Assert.Single(function.ColumnArgs).Column);
    }

    [Fact]
    public void TestSetOperationBranchRole()
    {
        var analysis = Polyglot.AnalyzeQuery("SELECT a FROM t UNION ALL SELECT a FROM u");

        var branches = Assert.Single(analysis.SetOperations).Branches;
        Assert.Equal(2, branches.Length);
        Assert.All(branches, b => Assert.Equal(SetOperationBranchRole.value, b.Role));
    }

    [Fact]
    public void TestColumnUsesFilter()
    {
        string sql = "SELECT o.id FROM orders AS o WHERE o.amount > 10";
        var analysis = Polyglot.AnalyzeQuery(sql);

        var use = Assert.Single(analysis.ColumnUses);
        Assert.Equal(ColumnUseContext.filter, use.Context);
        Assert.Equal("root", use.ScopePath);
        Assert.Equal("where_clause.this", use.ExpressionPath);
        Assert.Equal("o.amount > 10", use.ExpressionSql);

        var reference = Assert.Single(use.References);
        Assert.Equal("orders", reference.SourceName);
        Assert.Equal("o", reference.SourceAlias);
        Assert.Equal("amount", reference.Column);
        Assert.Equal(ReferenceConfidence.resolved, reference.Confidence);
        Assert.NotNull(reference.Span);
        Assert.Equal("o.amount", sql.Substring(reference.Span.Start, reference.Span.End - reference.Span.Start));
    }

    [Fact]
    public void TestColumnUsesEmpty()
    {
        Assert.Empty(Polyglot.AnalyzeQuery("SELECT 1").ColumnUses);
    }
}
