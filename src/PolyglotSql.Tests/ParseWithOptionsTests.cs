using PolyglotSql;
using PolyglotSql.Bundle;
using PolyglotSql.Models;

namespace PolyglotSql.Tests;

public class ParseWithOptionsTests
{
    static ParseWithOptionsTests()
    {
        BundleInitializer.Initialize();
    }

    [Fact]
    public void TestParseWithOptionsDefault()
    {
        var result = Polyglot.ParseWithOptions("SELECT a FROM t; SELECT b FROM u", Dialect.Generic, new ParseOptions());

        Assert.Equal(2, result.Length);
        Assert.Contains("SELECT a FROM t", Polyglot.GenerateOne(result[0]));
    }

    [Fact]
    public void TestParseWithOptionsNull()
    {
        var result = Polyglot.ParseWithOptions("SELECT 1");

        Assert.Single(result);
    }

    [Fact]
    public void TestParseWithOptionsGuardWithinLimit()
    {
        var options = new ParseOptions { ComplexityGuard = new ComplexityGuardOptions { MaxFunctionCallDepth = 2 } };
        var result = Polyglot.ParseWithOptions("SELECT ABS(ABS(1))", Dialect.Generic, options);

        Assert.Single(result);
    }

    [Fact]
    public void TestParseWithOptionsGuardExceeded()
    {
        var options = new ParseOptions { ComplexityGuard = new ComplexityGuardOptions { MaxFunctionCallDepth = 1 } };

        var ex = Assert.Throws<PolyglotException>(() => Polyglot.ParseWithOptions("SELECT ABS(ABS(1))", Dialect.Generic, options));
        Assert.Contains("E_GUARD_FUNCTION_NESTING_DEPTH_EXCEEDED", ex.Message);
    }
}
