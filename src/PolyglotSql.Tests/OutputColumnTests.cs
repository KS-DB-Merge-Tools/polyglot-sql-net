using System.Text.Json;
using PolyglotSql;
using PolyglotSql.Models;

namespace PolyglotSql.Tests;

// Pure serialization tests - no native library required
public class OutputColumnTests
{
    [Fact]
    public void TestNamed()
    {
        var column = JsonSerializer.Deserialize("{\"kind\":\"named\",\"name\":\"a\",\"ordinal\":3}", PolyglotJsonContext.Default.OutputColumn);

        var named = Assert.IsType<OutputColumn.Named>(column);
        Assert.Equal("a", named.Name);
        Assert.Equal(3, named.Ordinal);
    }

    [Fact]
    public void TestUnnamed()
    {
        var column = JsonSerializer.Deserialize("{\"kind\":\"unnamed\",\"ordinal\":null}", PolyglotJsonContext.Default.OutputColumn);

        var unnamed = Assert.IsType<OutputColumn.Unnamed>(column);
        Assert.Null(unnamed.Ordinal);
    }

    [Fact]
    public void TestWildcard()
    {
        var column = JsonSerializer.Deserialize("{\"kind\":\"wildcard\",\"qualifier\":\"t\",\"startOrdinal\":1}", PolyglotJsonContext.Default.OutputColumn);

        var wildcard = Assert.IsType<OutputColumn.Wildcard>(column);
        Assert.Equal("t", wildcard.Qualifier);
        Assert.Equal(1, wildcard.StartOrdinal);
    }

    [Fact]
    public void TestQueryOutput()
    {
        string json = "{\"columns\":[{\"kind\":\"named\",\"name\":\"a\",\"ordinal\":0},{\"kind\":\"wildcard\",\"qualifier\":null,\"startOrdinal\":1}],\"ordinalComplete\":false}";
        var output = JsonSerializer.Deserialize(json, PolyglotJsonContext.Default.QueryOutput)!;

        Assert.False(output.OrdinalComplete);
        Assert.Equal(2, output.Columns.Count);
        Assert.IsType<OutputColumn.Named>(output.Columns[0]);
        Assert.IsType<OutputColumn.Wildcard>(output.Columns[1]);
        Assert.Equal(json, JsonSerializer.Serialize(output, PolyglotJsonContext.Default.QueryOutput));
    }
}
