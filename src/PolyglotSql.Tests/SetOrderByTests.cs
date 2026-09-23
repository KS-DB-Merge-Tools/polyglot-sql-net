using PolyglotSql;
using PolyglotSql.Bundle;
using PolyglotSql.Models;

namespace PolyglotSql.Tests;

public class SetOrderByTests
{
    static SetOrderByTests()
    {
        BundleInitializer.Initialize();
    }

    [Fact]
    public void TestSetOrderBy()
    {
        var ast = Polyglot.Parse("SELECT a, b FROM t");
        var orderBy = Polyglot.Parse("b");
        var result = Polyglot.SetOrderBy(ast, orderBy);
        string sql = Polyglot.GenerateOne(result[0]);
        Console.WriteLine($"Result: {sql}");

        Assert.Contains("ORDER BY b", sql);
    }

    [Fact]
    public void TestSetOrderByMultipleColumns()
    {
        var ast = Polyglot.Parse("SELECT a, b FROM t");
        var orderBy = new[] { Polyglot.ParseOne("b"), Polyglot.ParseOne("a") };
        var result = Polyglot.SetOrderBy(ast, orderBy);
        string sql = Polyglot.GenerateOne(result[0]);
        Console.WriteLine($"Result: {sql}");

        Assert.Contains("ORDER BY b, a", sql);
    }
}
