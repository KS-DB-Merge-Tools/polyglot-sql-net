using PolyglotSql;
using PolyglotSql.Bundle;

namespace SqlGlotDotNet.DebugTest;

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
            string json = Polyglot.Parse("SELECT 1");
            Console.WriteLine($"AST JSON length: {json.Length}");
            Console.WriteLine($"AST JSON preview: {json.Substring(0, Math.Min(200, json.Length))}...");

            var nodes = AstParser.Parse(json);
            Console.WriteLine($"Parsed {nodes.Count} AST nodes");
            foreach (var node in nodes)
            {
                Console.WriteLine($"  Node: {node}");
            }

            Assert.False(string.IsNullOrEmpty(json), "JSON should not be empty");
            Assert.True(nodes.Count > 0, "Should have at least one AST node");
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
            string json = Polyglot.Parse(sql);
            Console.WriteLine($"SQL: {sql}");
            Console.WriteLine($"AST JSON length: {json.Length}");

            var nodes = AstParser.Parse(json);
            Console.WriteLine($"Parsed {nodes.Count} AST nodes");
            foreach (var node in nodes)
            {
                Console.WriteLine($"  Node: {node}");
            }

            Assert.True(nodes.Count > 0, "Should have at least one AST node");
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
            string json = Polyglot.ParseOne("SELECT 1");
            Console.WriteLine($"ParseOne JSON: {json.Substring(0, Math.Min(200, json.Length))}...");
            Assert.False(string.IsNullOrEmpty(json), "JSON should not be empty");
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
            string json = Polyglot.Parse(sql);
            Console.WriteLine($"SQL: {sql}");
            Console.WriteLine($"AST JSON length: {json.Length}");

            var nodes = AstParser.Parse(json);
            Console.WriteLine($"Parsed {nodes.Count} AST nodes");
            foreach (var node in nodes)
            {
                Console.WriteLine($"  Node: {node}");
            }

            Assert.True(nodes.Count > 0, "Should have at least one AST node");
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
            string json = Polyglot.Parse(sql);
            Console.WriteLine($"SQL: {sql}");
            Console.WriteLine($"AST JSON length: {json.Length}");

            var nodes = AstParser.Parse(json);
            Console.WriteLine($"Parsed {nodes.Count} AST nodes");
            foreach (var node in nodes)
            {
                Console.WriteLine($"  Node: {node}");
            }

            Assert.True(nodes.Count >= 2, "Should have at least 2 AST nodes for 2 statements");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: {ex.GetType().Name}: {ex.Message}");
            throw;
        }
    }
}
