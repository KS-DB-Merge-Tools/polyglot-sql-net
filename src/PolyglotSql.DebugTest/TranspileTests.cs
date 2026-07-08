using PolyglotSql;
using PolyglotSql.Bundle;
using PolyglotSql.Models;

namespace SqlGlotDotNet.DebugTest;

public class TranspileTests
{
    static TranspileTests()
    {
        BundleInitializer.Initialize();
    }

    [Fact]
    public void TestTranspileBasic()
    {
        Console.WriteLine("=== TestTranspileBasic ===");
        try
        {
            string[] result = Polyglot.Transpile("SELECT 1", Dialect.Generic, Dialect.Generic);
            Console.WriteLine($"Result count: {result.Length}");
            foreach (var r in result)
                Console.WriteLine($"  Item: {r}");
            Assert.NotEmpty(result);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: {ex.GetType().Name}: {ex.Message}");
            throw;
        }
    }

    [Fact]
    public void TestTranspileSelect()
    {
        Console.WriteLine("=== TestTranspileSelect ===");
        try
        {
            string[] result = Polyglot.Transpile("SELECT a, b FROM t", Dialect.Generic, Dialect.MySQL);
            Console.WriteLine($"Result count: {result.Length}");
            foreach (var r in result)
                Console.WriteLine($"  Item: {r}");
            Assert.NotEmpty(result);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: {ex.GetType().Name}: {ex.Message}");
            throw;
        }
    }

    [Fact]
    public void TestTranspileWithOptions()
    {
        Console.WriteLine("=== TestTranspileWithOptions ===");
        try
        {
            string optionsJson = "{\"pretty\": true}";
            string[] result = Polyglot.TranspileWithOptions("SELECT 1", Dialect.Generic, Dialect.Generic, optionsJson);
            Console.WriteLine($"Result count: {result.Length}");
            foreach (var r in result)
                Console.WriteLine($"  Item: {r}");
            Assert.NotEmpty(result);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: {ex.GetType().Name}: {ex.Message}");
            throw;
        }
    }

    [Fact]
    public void TestTranspileDialectConversion()
    {
        Console.WriteLine("=== TestTranspileDialectConversion ===");
        try
        {
            string sql = "SELECT * FROM users WHERE id = 1";
            string[] result = Polyglot.Transpile(sql, Dialect.MySQL, Dialect.PostgreSQL);
            Console.WriteLine($"Result count: {result.Length}");
            foreach (var r in result)
                Console.WriteLine($"  Item: {r}");
            Assert.NotEmpty(result);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: {ex.GetType().Name}: {ex.Message}");
            throw;
        }
    }

    [Fact]
    public void TestTranspileComplexQuery()
    {
        Console.WriteLine("=== TestTranspileComplexQuery ===");
        try
        {
            string sql = "SELECT COUNT(*) FROM users GROUP BY status ORDER BY count DESC LIMIT 10";
            string[] result = Polyglot.Transpile(sql, Dialect.Generic, Dialect.BigQuery);
            Console.WriteLine($"Result count: {result.Length}");
            foreach (var r in result)
                Console.WriteLine($"  Item: {r}");
            Assert.NotEmpty(result);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: {ex.GetType().Name}: {ex.Message}");
            throw;
        }
    }

    [Fact]
    public void TestTranspileError()
    {
        Console.WriteLine("=== TestTranspileError ===");
        try
        {
            string[] result = Polyglot.Transpile("SELECT <<--", Dialect.Generic, Dialect.Generic);
            Console.WriteLine($"Result count (error case): {result.Length}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Expected error: {ex.GetType().Name}: {ex.Message}");
        }
    }
}
