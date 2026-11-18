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

    // Same schema as the native path_kind_analysis test helper
    private static QueryAnalysis AnalyzeOrders(string sql) => Polyglot.AnalyzeQuery(sql, new AnalyzeQueryOptions
    {
        Dialect = Dialect.PostgreSQL,
        Schema = new ValidationSchema
        {
            Tables =
            {
                new SchemaTable
                {
                    Name = "orders",
                    Schema = "s",
                    Columns =
                    {
                        new SchemaColumn { Name = "order_id", Type = "INT" },
                        new SchemaColumn { Name = "customer_id", Type = "INT" },
                        new SchemaColumn { Name = "amount", Type = "DECIMAL(10,2)" },
                        new SchemaColumn { Name = "status", Type = "TEXT" },
                        new SchemaColumn { Name = "ordered_at", Type = "TIMESTAMP" }
                    }
                }
            }
        }
    });

    [Fact]
    public void TestUpstreamPathTransformKindAndFunctions()
    {
        var analysis = AnalyzeOrders(
            "WITH t AS (SELECT customer_id, SUM(amount) AS total FROM s.orders GROUP BY customer_id) " +
            "SELECT customer_id, ROUND(total, 2) AS total FROM t");

        var customer = Assert.Single(analysis.Projections[0].Upstream);
        Assert.Equal("customer_id", customer.Column, ignoreCase: true);
        Assert.Equal(TransformKind.direct, customer.PathTransformKind);
        Assert.Empty(customer.PathFunctions);
        Assert.False(customer.PathWindow);

        var total = Assert.Single(analysis.Projections[1].Upstream);
        Assert.Equal("amount", total.Column, ignoreCase: true);
        Assert.Equal(TransformKind.aggregation, total.PathTransformKind);
        Assert.Equal(new[] { "ROUND", "SUM" }, total.PathFunctions);
        Assert.False(total.PathWindow);
    }

    [Fact]
    public void TestUpstreamPathWindow()
    {
        var analysis = AnalyzeOrders("SELECT ROW_NUMBER() OVER (ORDER BY ordered_at) AS rn FROM s.orders");

        var reference = Assert.Single(Assert.Single(analysis.Projections).Upstream);
        Assert.Equal("ordered_at", reference.Column, ignoreCase: true);
        Assert.Equal(new[] { "ROW_NUMBER" }, reference.PathFunctions);
        Assert.True(reference.PathWindow);
    }

    [Fact]
    public void TestUpstreamPathCast()
    {
        var analysis = AnalyzeOrders("WITH a AS (SELECT CAST(amount AS INT) AS amt FROM s.orders) SELECT amt FROM a");

        var projection = Assert.Single(analysis.Projections);
        Assert.Equal(TransformKind.direct, projection.TransformKind);
        var reference = Assert.Single(projection.Upstream);
        Assert.Equal(TransformKind.cast, reference.PathTransformKind);
        Assert.Empty(reference.PathFunctions);
    }

    [Fact]
    public void TestColumnUseReferencesHaveNoPath()
    {
        var analysis = AnalyzeOrders("SELECT t.total FROM (SELECT SUM(amount) AS total FROM s.orders) AS t WHERE t.total > 0");

        Assert.Equal(TransformKind.aggregation, Assert.Single(analysis.Projections[0].Upstream).PathTransformKind);

        var use = Assert.Single(analysis.ColumnUses, u => u.Context == ColumnUseContext.filter);
        Assert.All(use.References, r =>
        {
            Assert.Null(r.PathTransformKind);
            Assert.Empty(r.PathFunctions);
            Assert.False(r.PathWindow);
        });
    }
}
