using PolyglotSql;
using PolyglotSql.Bundle;
using PolyglotSql.Models;

namespace PolyglotSql.Tests;

public class SetLimitTests
{
    static SetLimitTests()
    {
        BundleInitializer.Initialize();
    }

    [Fact]
    public void TestSetLimit()
    {
        var ast = Polyglot.Parse("SELECT a FROM t");
        var result = Polyglot.SetLimit(ast, 10);
        string sql = Polyglot.GenerateOne(result[0]);
        Console.WriteLine($"Result: {sql}");

        Assert.Equal("SELECT a FROM t LIMIT 10", sql);
    }

    [Fact]
    public void TestSetLimitReplacesExisting()
    {
        var ast = Polyglot.Parse("SELECT a FROM t LIMIT 5");
        var result = Polyglot.SetLimit(ast, 20);
        string sql = Polyglot.GenerateOne(result[0]);
        Console.WriteLine($"Result: {sql}");

        Assert.Contains("LIMIT 20", sql);
        Assert.DoesNotContain("LIMIT 5", sql);
    }

    [Fact]
    public void TestSetLimitTranspiled()
    {
        var ast = Polyglot.Parse("SELECT a FROM t");
        var result = Polyglot.SetLimit(ast, 10);
        string sql = Polyglot.GenerateOne(result[0], Dialect.TSQL);
        Console.WriteLine($"Result: {sql}");

        Assert.Contains("TOP 10", sql);
    }
}
