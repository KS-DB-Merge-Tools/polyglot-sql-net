using System.Text.Json;
using PolyglotSql;
using PolyglotSql.Models;

namespace PolyglotSql.Tests;

// Pure serialization tests - no native library required
public class SchemaValidationOptionsTests
{
    private static string Serialize(SchemaValidationOptions options)
        => JsonSerializer.Serialize(options, PolyglotJsonContext.Default.SchemaValidationOptions);

    [Fact]
    public void TestDefault()
    {
        Assert.Equal("{\"check_types\":false,\"check_references\":false,\"semantic\":false,\"strict_syntax\":false}",
            Serialize(new SchemaValidationOptions()));
    }

    [Fact]
    public void TestCheckTypes()
    {
        Assert.Contains("\"check_types\":true", Serialize(new SchemaValidationOptions { CheckTypes = true }));
    }

    [Fact]
    public void TestCheckReferences()
    {
        Assert.Contains("\"check_references\":true", Serialize(new SchemaValidationOptions { CheckReferences = true }));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void TestStrict(bool strict)
    {
        Assert.Contains($"\"strict\":{strict.ToString().ToLowerInvariant()}", Serialize(new SchemaValidationOptions { Strict = strict }));
    }

    [Fact]
    public void TestSemantic()
    {
        Assert.Contains("\"semantic\":true", Serialize(new SchemaValidationOptions { Semantic = true }));
    }

    [Fact]
    public void TestStrictSyntax()
    {
        Assert.Contains("\"strict_syntax\":true", Serialize(new SchemaValidationOptions { StrictSyntax = true }));
    }

    [Fact]
    public void TestComplexityGuard()
    {
        var options = new SchemaValidationOptions { ComplexityGuard = new ComplexityGuardOptions { MaxAstNodes = 5 } };

        Assert.Contains("\"complexity_guard\":{\"maxAstNodes\":5}", Serialize(options));
    }
}
