using PolyglotSql;
using PolyglotSql.Bundle;
using PolyglotSql.Models;

namespace PolyglotSql.Tests;

public class SetOffsetTests
{
    static SetOffsetTests()
    {
        BundleInitializer.Initialize();
    }

    [Fact]
    public void TestSetOffset()
    {
        var ast = Polyglot.Parse("SELECT a FROM t LIMIT 10");
        var result = Polyglot.SetOffset(ast, 5);
        string sql = Polyglot.GenerateOne(result[0]);
        Console.WriteLine($"Result: {sql}");

        Assert.Contains("LIMIT 10", sql);
        Assert.Contains("OFFSET 5", sql);
    }

    [Fact]
    public void TestSetOffsetMultipleStatements()
    {
        var ast = Polyglot.Parse("SELECT a FROM t; SELECT b FROM u");
        var result = Polyglot.SetOffset(ast, 3);

        Assert.Equal(2, result.Length);
        Assert.Contains("OFFSET 3", Polyglot.GenerateOne(result[0]));
        Assert.Contains("OFFSET 3", Polyglot.GenerateOne(result[1]));
    }
}
