using PolyglotSql;
using PolyglotSql.Bundle;
using PolyglotSql.Models;

namespace PolyglotSql.Tests;

public class OutputColumnsTests
{
    static OutputColumnsTests()
    {
        BundleInitializer.Initialize();
    }

    [Fact]
    public void TestOutputColumnsNamed()
    {
        var output = Polyglot.OutputColumns("SELECT a, b AS c FROM t");

        Assert.True(output.OrdinalComplete);
        Assert.Equal(2, output.Columns.Count);

        var first = Assert.IsType<OutputColumn.Named>(output.Columns[0]);
        Assert.Equal("a", first.Name);
        Assert.Equal(0, first.Ordinal);

        var second = Assert.IsType<OutputColumn.Named>(output.Columns[1]);
        Assert.Equal("c", second.Name);
        Assert.Equal(1, second.Ordinal);
    }

    [Fact]
    public void TestOutputColumnsWildcard()
    {
        var output = Polyglot.OutputColumns("SELECT * FROM t");

        Assert.False(output.OrdinalComplete);
        var wildcard = Assert.IsType<OutputColumn.Wildcard>(Assert.Single(output.Columns));
        Assert.Null(wildcard.Qualifier);
        Assert.Equal(0, wildcard.StartOrdinal);
    }

    [Fact]
    public void TestOutputColumnsQualifiedWildcard()
    {
        var output = Polyglot.OutputColumns("SELECT a, t.* FROM t");

        Assert.Equal(2, output.Columns.Count);
        Assert.IsType<OutputColumn.Named>(output.Columns[0]);
        var wildcard = Assert.IsType<OutputColumn.Wildcard>(output.Columns[1]);
        Assert.Equal("t", wildcard.Qualifier);
        Assert.Equal(1, wildcard.StartOrdinal);
    }
}
