using PolyglotSql;
using PolyglotSql.Bundle;

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
        Console.WriteLine("=== TestParseBasic ===");
        try
        {
            var exprs = Polyglot.Parse("SELECT 1");
            Console.WriteLine($"Parsed {exprs.Length} AST nodes");
            foreach (var expr in exprs)
            {
                Console.WriteLine($"  Node JSON: {expr.ToJsonString().Substring(0, Math.Min(100, expr.ToJsonString().Length))}...");
            }

            Assert.True(exprs.Length > 0, "Should have at least one AST node");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: {ex.GetType().Name}: {ex.Message}");
            throw;
        }
    }

    [Fact]
    public void TestParseSelectWithColumns()
    {
        Console.WriteLine("=== TestParseSelectWithColumns ===");
        try
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
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: {ex.GetType().Name}: {ex.Message}");
            throw;
        }
    }

    [Fact]
    public void TestParseOne()
    {
        Console.WriteLine("=== TestParseOne ===");
        try
        {
            var expr = Polyglot.ParseOne("SELECT 1");
            Console.WriteLine($"ParseOne JSON: {expr.ToJsonString().Substring(0, Math.Min(200, expr.ToJsonString().Length))}...");
            Assert.False(string.IsNullOrEmpty(expr.ToJsonString()), "JSON should not be empty");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: {ex.GetType().Name}: {ex.Message}");
            throw;
        }
    }

    [Fact]
    public void TestParseComplexQuery()
    {
        Console.WriteLine("=== TestParseComplexQuery ===");
        try
        {
            string sql = "SELECT COUNT(*) FROM users GROUP BY status ORDER BY count DESC LIMIT 10";
            var exprs = Polyglot.Parse(sql);
            Console.WriteLine($"SQL: {sql}");
            Console.WriteLine($"Parsed {exprs.Length} AST nodes");

            Assert.True(exprs.Length > 0, "Should have at least one AST node");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: {ex.GetType().Name}: {ex.Message}");
            throw;
        }
    }

    [Fact]
    public void TestParseMultipleStatements()
    {
        Console.WriteLine("=== TestParseMultipleStatements ===");
        try
        {
            string sql = "SELECT 1; SELECT 2";
            var exprs = Polyglot.Parse(sql);
            Console.WriteLine($"SQL: {sql}");
            Console.WriteLine($"Parsed {exprs.Length} AST nodes");

            Assert.True(exprs.Length >= 2, "Should have at least 2 AST nodes for 2 statements");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: {ex.GetType().Name}: {ex.Message}");
            throw;
        }
    }

    [Fact]
    public void TestAnnotateTypes()
    {
        Console.WriteLine("=== TestAnnotateTypes ===");
        try
        {
            string sql = "SELECT 1 + 1 AS x";
            var annotated = Polyglot.AnnotateTypes(sql);
            Console.WriteLine($"Annotated {annotated.Length} node(s)");
            foreach (var node in annotated)
                Console.WriteLine(node.ToJsonString().Substring(0, Math.Min(120, node.ToJsonString().Length)) + "...");

            Assert.True(annotated.Length > 0, "Should have at least one AST node");
            Assert.Contains(annotated, n => n.ToJsonString().Contains("inferred_type"));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: {ex.GetType().Name}: {ex.Message}");
            throw;
        }
    }

    [Fact]
    public void TestGenerate()
    {
        Console.WriteLine("=== TestGenerate ===");
        try
        {
            string sql = "SELECT 1";
            var ast = Polyglot.Parse(sql);
            var generated = Polyglot.Generate(ast);
            Console.WriteLine($"Generated {generated.Length} statement(s): {string.Join(" ; ", generated)}");

            Assert.True(generated.Length == 1, "Should generate exactly one statement");
            Assert.Contains("SELECT", generated[0]);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: {ex.GetType().Name}: {ex.Message}");
            throw;
        }
    }
}
