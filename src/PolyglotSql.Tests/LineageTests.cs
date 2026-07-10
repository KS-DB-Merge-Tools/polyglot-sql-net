using PolyglotSql;
using PolyglotSql.Bundle;
using PolyglotSql.Models;

namespace PolyglotSql.Tests;

public class LineageTests
{
    static LineageTests()
    {
        BundleInitializer.Initialize();
    }

    [Fact]
    public void TestLineageSimple()
    {
        Console.WriteLine("=== TestLineageSimple ===");
        try
        {
            string sql = "SELECT a FROM t";
            var node = Polyglot.Lineage("a", sql);
            Console.WriteLine($"SQL: {sql}");
            Console.WriteLine($"Lineage: {node}");

            Assert.NotNull(node);
            Assert.Equal("a", node.Name);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: {ex.GetType().Name}: {ex.Message}");
            throw;
        }
    }

    [Fact]
    public void TestLineageDerivedColumn()
    {
        Console.WriteLine("=== TestLineageDerivedColumn ===");
        try
        {
            string sql = "SELECT a + 1 AS b FROM t";
            var node = Polyglot.Lineage("b", sql);
            Console.WriteLine($"SQL: {sql}");
            Console.WriteLine($"Lineage: {node}");

            Assert.NotNull(node);
            Assert.Equal("b", node.Name);
            Assert.True(node.Downstream.Count > 0, "Derived column should have downstream sources");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: {ex.GetType().Name}: {ex.Message}");
            throw;
        }
    }

    [Fact]
    public void TestLineageWithSchema()
    {
        Console.WriteLine("=== TestLineageWithSchema ===");
        try
        {
            string sql = "SELECT a FROM t";
            var schema = new ValidationSchema
            {
                Tables =
                {
                    new SchemaTable
                    {
                        Name = "t",
                        Columns = { new SchemaColumn { Name = "a", Type = "int" } }
                    }
                }
            };
            var node = Polyglot.LineageWithSchema("a", sql, schema);
            Console.WriteLine($"SQL: {sql}");
            Console.WriteLine($"Lineage: {node}");

            Assert.NotNull(node);
            Assert.Equal("a", node.Name);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: {ex.GetType().Name}: {ex.Message}");
            throw;
        }
    }

    [Fact]
    public void TestLineageDeserialization()
    {
        Console.WriteLine("=== TestLineageDeserialization ===");
        try
        {
            string sql = "SELECT week_start FROM UNNEST(GENERATE_DATE_ARRAY('2024-01-01', '2024-12-31', INTERVAL 1 WEEK)) AS date_val";
            var node = Polyglot.Lineage("week_start", sql, Dialect.BigQuery);
            Console.WriteLine($"SQL: {sql}");
            Console.WriteLine($"Lineage: {node}");

            Assert.NotNull(node);
            Assert.Equal("week_start", node.Name);
            Assert.Equal(SourceKind.root, node.SourceKind);
            Assert.Equal(string.Empty, node.SourceName);
            Assert.Equal(string.Empty, node.ReferenceNodeName);
            Assert.NotNull(node.Expression);
            Assert.Contains("week_start", node.Expression.ToJsonString());
            Assert.NotNull(node.Source);
            Assert.NotEmpty(node.Downstream);

            var child = node.Downstream[0];
            Assert.Equal("_0.week_start", child.Name);
            Assert.Equal("_0", child.SourceName);
            Assert.Equal(SourceKind.@virtual, child.SourceKind);
            Assert.Equal("date_val", child.SourceAlias);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: {ex.GetType().Name}: {ex.Message}");
            throw;
        }
    }

    [Fact]
    public void TestLineageDerivedTableSourceKind()
    {
        Console.WriteLine("=== TestLineageDerivedTableSourceKind ===");
        try
        {
            string sql = "SELECT a FROM (SELECT a FROM t) sub";
            var node = Polyglot.Lineage("a", sql);
            Console.WriteLine($"SQL: {sql}");
            Console.WriteLine($"Lineage: {node}");

            Assert.NotNull(node);
            Assert.Equal("a", node.Name);
            Assert.Equal(SourceKind.root, node.SourceKind);
            Assert.NotEmpty(node.Downstream);

            var derived = node.Downstream[0];
            Assert.Equal("a", derived.Name);
            Assert.Equal("sub", derived.SourceName);
            Assert.Equal(SourceKind.derived_table, derived.SourceKind);
            Assert.Equal("a", derived.ReferenceNodeName);
            Assert.NotEmpty(derived.Downstream);

            var leaf = derived.Downstream[0];
            Assert.Equal("t.a", leaf.Name);
            Assert.Equal("t", leaf.SourceName);
            Assert.Equal(SourceKind.table, leaf.SourceKind);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: {ex.GetType().Name}: {ex.Message}");
            throw;
        }
    }
}
