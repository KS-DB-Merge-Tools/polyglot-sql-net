using System.Linq;
using PolyglotSql;
using PolyglotSql.Bundle;
using PolyglotSql.Models;

namespace PolyglotSql.Tests;

public class ValidateWithOptionsTests
{
    static ValidateWithOptionsTests()
    {
        BundleInitializer.Initialize();
    }

    [Fact]
    public void TestValidateWithOptionsDefault()
    {
        var result = Polyglot.ValidateWithOptions("SELECT a FROM t", Dialect.Generic, new ValidationOptions());

        Assert.True(result.Valid);
    }

    [Fact]
    public void TestValidateWithOptionsSemanticWarning()
    {
        var result = Polyglot.ValidateWithOptions("SELECT * FROM t", Dialect.Generic, new ValidationOptions { Semantic = true });

        Assert.Contains(result.Errors, e => e.Code == "W001" && e.Severity == ValidationSeverity.warning);
    }

    [Fact]
    public void TestValidateWithOptionsNoSemanticWarningByDefault()
    {
        var result = Polyglot.ValidateWithOptions("SELECT * FROM t");

        Assert.DoesNotContain(result.Errors, e => e.Code == "W001");
    }

    [Fact]
    public void TestValidateWithOptionsStrictSyntax()
    {
        string sql = "SELECT a, FROM t";

        var lenient = Polyglot.ValidateWithOptions(sql, Dialect.Generic, new ValidationOptions());
        var strict = Polyglot.ValidateWithOptions(sql, Dialect.Generic, new ValidationOptions { StrictSyntax = true });

        Assert.True(lenient.Valid);
        Assert.False(strict.Valid);
    }

    [Fact]
    public void TestValidateWithOptionsGuardExceeded()
    {
        var options = new ValidationOptions { ComplexityGuard = new ComplexityGuardOptions { MaxFunctionCallDepth = 1 } };
        var result = Polyglot.ValidateWithOptions("SELECT ABS(ABS(1))", Dialect.Generic, options);

        Assert.False(result.Valid);
        Assert.Contains(result.Errors, e => e.Message.Contains("E_GUARD_FUNCTION_NESTING_DEPTH_EXCEEDED"));
    }
}
