using System.Text.Json;
using PolyglotSql;
using PolyglotSql.Models;

namespace PolyglotSql.Tests;

// Pure serialization tests - no native library required
public class ValidationOptionsTests
{
    private static string Serialize(ValidationOptions options)
        => JsonSerializer.Serialize(options, PolyglotJsonContext.Default.ValidationOptions);

    [Fact]
    public void TestDefault()
    {
        Assert.Equal("{\"strictSyntax\":false,\"semantic\":false}", Serialize(new ValidationOptions()));
    }

    [Fact]
    public void TestStrictSyntax()
    {
        Assert.Contains("\"strictSyntax\":true", Serialize(new ValidationOptions { StrictSyntax = true }));
    }

    [Fact]
    public void TestSemantic()
    {
        Assert.Contains("\"semantic\":true", Serialize(new ValidationOptions { Semantic = true }));
    }

    [Fact]
    public void TestComplexityGuard()
    {
        var options = new ValidationOptions { ComplexityGuard = new ComplexityGuardOptions { MaxFunctionCallDepth = 128 } };

        Assert.Contains("\"complexityGuard\":{\"maxFunctionCallDepth\":128}", Serialize(options));
    }
}
