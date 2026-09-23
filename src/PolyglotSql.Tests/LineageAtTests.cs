using PolyglotSql;
using PolyglotSql.Bundle;
using PolyglotSql.Models;

namespace PolyglotSql.Tests;

public class LineageAtTests
{
    static LineageAtTests()
    {
        BundleInitializer.Initialize();
    }

    [Fact]
    public void TestLineageAtFirstColumn()
    {
        var node = Polyglot.LineageAt(0, "SELECT a, b + 1 AS c FROM t");
        Console.WriteLine($"Lineage: {node}");

        Assert.NotNull(node);
        Assert.Equal("a", node.Name);
    }

    [Fact]
    public void TestLineageAtDerivedColumn()
    {
        var node = Polyglot.LineageAt(1, "SELECT a, b + 1 AS c FROM t");
        Console.WriteLine($"Lineage: {node}");

        Assert.NotNull(node);
        Assert.Equal("c", node.Name);
        Assert.NotEmpty(node.Downstream);
    }

    [Fact]
    public void TestLineageAtOrdinalOutOfRange()
    {
        Assert.Throws<PolyglotException>(() => Polyglot.LineageAt(5, "SELECT a FROM t"));
    }

    [Fact]
    public void TestLineageAtNegativeOrdinal()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Polyglot.LineageAt(-1, "SELECT a FROM t"));
    }
}
