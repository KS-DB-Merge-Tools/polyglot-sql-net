using PolyglotSql;
using PolyglotSql.Bundle;
using PolyglotSql.Models;

namespace PolyglotSql.Tests;

public class ParseDataTypeWithOptionsTests
{
    static ParseDataTypeWithOptionsTests()
    {
        BundleInitializer.Initialize();
    }

    [Fact]
    public void TestParseDataTypeWithOptionsDefault()
    {
        var result = Polyglot.ParseDataTypeWithOptions("VARCHAR(100)", Dialect.Generic, new ParseOptions());

        var varChar = Assert.IsType<DataType.VarChar>(result);
        Assert.Equal(100u, varChar.Length);
    }

    [Fact]
    public void TestParseDataTypeWithOptionsGuardExceeded()
    {
        var options = new ParseOptions { ComplexityGuard = new ComplexityGuardOptions { MaxInputBytes = 3 } };

        var ex = Assert.Throws<PolyglotException>(() => Polyglot.ParseDataTypeWithOptions("VARCHAR(100)", Dialect.Generic, options));
        Assert.Contains("E_GUARD_INPUT_TOO_LARGE", ex.Message);
    }
}
