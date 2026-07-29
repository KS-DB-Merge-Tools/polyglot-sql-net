using PolyglotSql;
using PolyglotSql.Bundle;
using PolyglotSql.Models;
using Xunit;

namespace PolyglotSql.Tests;

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
            string[] result = Polyglot.TranspileWithOptions("SELECT 1", Dialect.Generic, Dialect.Generic, new TranspileOptions { Pretty = true });
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

    [Theory]
    [InlineData(true, true)]
    [InlineData(false, false)]
    public void TranspileOptionPretty(bool pretty, bool expectMultiline)
    {
        var opts = new TranspileOptions { Pretty = pretty };
        string[] result = Polyglot.TranspileWithOptions("SELECT 1", Dialect.Generic, Dialect.Generic, opts);
        Assert.NotEmpty(result);
        bool multiline = result.Any(r => r.Contains('\n'));
        Assert.Equal(expectMultiline, multiline);
    }

    [Theory]
    [InlineData(UnsupportedLevel.warn, false)]
    [InlineData(UnsupportedLevel.raise, true)]
    public void TranspileOptionUnsupportedLevel(UnsupportedLevel level, bool expectThrow)
    {
        var opts = new TranspileOptions { UnsupportedLevel = level };
        string sql = "SELECT JSONB_BUILD_OBJECT('a', 1) FROM t";
        if (expectThrow)
        {
            Assert.Throws<PolyglotException>(() =>
                Polyglot.TranspileWithOptions(sql, Dialect.PostgreSQL, Dialect.MySQL, opts));
        }
        else
        {
            string[] result = Polyglot.TranspileWithOptions(sql, Dialect.PostgreSQL, Dialect.MySQL, opts);
            Assert.NotEmpty(result);
        }
    }

    [Theory]
    [InlineData(1, true)]
    [InlineData(10, false)]
    public void TranspileOptionMaxUnsupported(int maxUnsupported, bool expectTruncated)
    {
        var opts = new TranspileOptions { UnsupportedLevel = UnsupportedLevel.raise, MaxUnsupported = maxUnsupported };
        string sql = "SELECT JSONB_BUILD_OBJECT('a', 1), TO_TSVECTOR('b') FROM t LATERAL JOIN u ON t.id = u.id";
        var ex = Assert.Throws<PolyglotException>(() =>
            Polyglot.TranspileWithOptions(sql, Dialect.PostgreSQL, Dialect.SQLite, opts));
        bool truncated = ex.Message.Contains("more");
        Assert.Equal(expectTruncated, truncated);
    }
}
