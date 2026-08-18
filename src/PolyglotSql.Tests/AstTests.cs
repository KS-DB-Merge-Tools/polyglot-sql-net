using PolyglotSql;
using PolyglotSql.Bundle;
using System.Text.Json.Nodes;

namespace PolyglotSql.Tests;

public class AstTests
{
    static AstTests()
    {
        BundleInitializer.Initialize();
    }

    [Fact]
    public void TestParseBasic()
    {
        var exprs = Polyglot.Parse("SELECT 1");
        Console.WriteLine($"Parsed {exprs.Length} AST nodes");
        foreach (var expr in exprs)
        {
            Console.WriteLine($"  Node JSON: {expr.ToJsonString().Substring(0, Math.Min(100, expr.ToJsonString().Length))}...");
        }

        Assert.True(exprs.Length > 0, "Should have at least one AST node");
    }

    [Fact]
    public void TestParseSelectWithColumns()
    {
        string sql = "SELECT a, b, c FROM users WHERE id = 1";
        var exprs = Polyglot.Parse(sql);
        Console.WriteLine($"SQL: {sql}");
        Console.WriteLine($"Parsed {exprs.Length} AST nodes");
        foreach (var expr in exprs)
        {
            Console.WriteLine($"  Node JSON: {expr.ToJsonString().Substring(0, Math.Min(100, expr.ToJsonString().Length))}...");
        }

        Assert.True(exprs.Length > 0, "Should have at least one AST node");
    }

    [Fact]
    public void TestParseOne()
    {
        var expr = Polyglot.ParseOne("SELECT 1");
        Console.WriteLine($"ParseOne JSON: {expr.ToJsonString().Substring(0, Math.Min(200, expr.ToJsonString().Length))}...");
        Assert.False(string.IsNullOrEmpty(expr.ToJsonString()), "JSON should not be empty");
    }

    [Fact]
    public void TestParseComplexQuery()
    {
        string sql = "SELECT COUNT(*) FROM users GROUP BY status ORDER BY count DESC LIMIT 10";
        var exprs = Polyglot.Parse(sql);
        Console.WriteLine($"SQL: {sql}");
        Console.WriteLine($"Parsed {exprs.Length} AST nodes");

        Assert.True(exprs.Length > 0, "Should have at least one AST node");
    }

    [Fact]
    public void TestParseMultipleStatements()
    {
        string sql = "SELECT 1; SELECT 2";
        var exprs = Polyglot.Parse(sql);
        Console.WriteLine($"SQL: {sql}");
        Console.WriteLine($"Parsed {exprs.Length} AST nodes");

        Assert.True(exprs.Length >= 2, "Should have at least 2 AST nodes for 2 statements");
    }

    [Fact]
    public void TestAnnotateTypes()
    {
        string sql = "SELECT 1 + 1 AS x";
        var annotated = Polyglot.AnnotateTypes(sql);
        Console.WriteLine($"Annotated {annotated.Length} node(s)");
        foreach (var node in annotated)
            Console.WriteLine(node.ToJsonString().Substring(0, Math.Min(120, node.ToJsonString().Length)) + "...");

        Assert.True(annotated.Length > 0, "Should have at least one AST node");
        Assert.Contains(annotated, n => n.ToJsonString().Contains("inferred_type"));
    }

    [Fact]
    public void TestGenerate()
    {
        string sql = "SELECT 1";
        var ast = Polyglot.Parse(sql);
        var generated = Polyglot.Generate(ast);
        Console.WriteLine($"Generated {generated.Length} statement(s): {string.Join(" ; ", generated)}");

        Assert.True(generated.Length == 1, "Should generate exactly one statement");
        Assert.Contains("SELECT", generated[0]);
    }

    [Fact]
    public void TestTransform()
    {
        string sql = "SELECT [a], b FROM t";
        var dialect = Models.Dialect.TSQL;

        Models.Expression ast = Polyglot.ParseOne(sql, dialect);

        ast.TransformAll(node => {
            if (node is JsonObject obj && obj.ContainsKey("quoted"))
            {
                obj["quoted"] = true;
            }
        });

        var generated = Polyglot.GenerateOne(ast, dialect);
        Assert.Equal("SELECT [a], [b] FROM [t]", generated);
    }
}
