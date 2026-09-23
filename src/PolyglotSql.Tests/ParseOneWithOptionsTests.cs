using PolyglotSql;
using PolyglotSql.Bundle;
using PolyglotSql.Models;

namespace PolyglotSql.Tests;

public class ParseOneWithOptionsTests
{
    static ParseOneWithOptionsTests()
    {
        BundleInitializer.Initialize();
    }

    [Fact]
    public void TestParseOneWithOptionsDefault()
    {
        var result = Polyglot.ParseOneWithOptions("SELECT a FROM t", Dialect.Generic, new ParseOptions());

        Assert.NotNull(result);
        Assert.Equal("SELECT a FROM t", Polyglot.GenerateOne(result));
    }

    [Fact]
    public void TestParseOneWithOptionsMultipleStatements()
    {
        Assert.Throws<PolyglotException>(() => Polyglot.ParseOneWithOptions("SELECT 1; SELECT 2"));
    }

    [Fact]
    public void TestParseOneWithOptionsGuardExceeded()
    {
        var options = new ParseOptions { ComplexityGuard = new ComplexityGuardOptions { MaxInputBytes = 5 } };

        var ex = Assert.Throws<PolyglotException>(() => Polyglot.ParseOneWithOptions("SELECT a FROM t", Dialect.Generic, options));
        Assert.Contains("E_GUARD_INPUT_TOO_LARGE", ex.Message);
    }
}
