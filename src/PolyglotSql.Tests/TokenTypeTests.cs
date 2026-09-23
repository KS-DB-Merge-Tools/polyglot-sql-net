using System.Linq;
using System.Text.Json;
using PolyglotSql;
using PolyglotSql.Models;

namespace PolyglotSql.Tests;

// Pure serialization tests - no native library required
public class TokenTypeTests
{
    private static TokenType ReadTokenType(string wireName)
    {
        string json = $"{{\"token_type\":\"{wireName}\",\"text\":\"x\",\"span\":{{\"start\":0,\"end\":1,\"line\":1,\"column\":2}},\"comments\":[],\"trailing_comments\":[]}}";
        return JsonSerializer.Deserialize(json, PolyglotJsonContext.Default.Token)!.TokenType;
    }

    [Theory]
    // multi-word names (previously spelled without underscores and read as UNKNOWN)
    [InlineData("D_COLON", TokenType.D_COLON)]
    [InlineData("I_LIKE", TokenType.I_LIKE)]
    [InlineData("VAR_CHAR", TokenType.VAR_CHAR)]
    [InlineData("BIG_INT", TokenType.BIG_INT)]
    [InlineData("CURRENT_DATE_TIME", TokenType.CURRENT_DATE_TIME)]
    [InlineData("AGGREGATE_FUNCTION", TokenType.AGGREGATE_FUNCTION)]
    // previously missing
    [InlineData("AS", TokenType.AS)]
    [InlineData("BY", TokenType.BY)]
    [InlineData("CAST", TokenType.CAST)]
    [InlineData("LINE_COMMENT", TokenType.LINE_COMMENT)]
    [InlineData("EOF", TokenType.EOF)]
    // unchanged
    [InlineData("SELECT", TokenType.SELECT)]
    [InlineData("L_PAREN", TokenType.L_PAREN)]
    [InlineData("NUMBER", TokenType.NUMBER)]
    public void TestWireNames(string wireName, TokenType expected)
    {
        Assert.Equal(expected, ReadTokenType(wireName));
    }

    [Fact]
    public void TestUnknownNameFallsBackToUnknown()
    {
        Assert.Equal(TokenType.UNKNOWN, ReadTokenType("NO_SUCH_TOKEN"));
    }

    [Fact]
    public void TestGroupByAndGroupingSetsAreDistinct()
    {
        Assert.NotEqual(TokenType.GROUP_BY, TokenType.GROUPING_SETS);
        Assert.Equal(TokenType.GROUPING_SETS, ReadTokenType("GROUPING_SETS"));
        Assert.Equal("GROUPING_SETS", TokenType.GROUPING_SETS.ToString());
    }

    [Fact]
    public void TestAllValuesUnique()
    {
        var values = System.Enum.GetValues(typeof(TokenType)).Cast<int>().ToArray();

        Assert.Equal(values.Length, values.Distinct().Count());
    }

    [Fact]
    public void TestWriteUsesWireName()
    {
        var token = new Token { TokenType = TokenType.D_COLON, Text = "::", Span = new Span(0, 2, 1, 3) };

        Assert.Contains("\"token_type\":\"D_COLON\"", JsonSerializer.Serialize(token, PolyglotJsonContext.Default.Token));
    }
}
