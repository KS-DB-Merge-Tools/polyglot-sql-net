using System.Text.Json;
using PolyglotSql;
using PolyglotSql.Models;

namespace PolyglotSql.Tests;

// Pure serialization tests - no native library required
public class ComplexityGuardOptionsTests
{
    private static string Serialize(ComplexityGuardOptions options)
        => JsonSerializer.Serialize(options, PolyglotJsonContext.Default.ComplexityGuardOptions);

    [Fact]
    public void TestEmptyOptionsSerializeToEmptyObject()
    {
        Assert.Equal("{}", Serialize(new ComplexityGuardOptions()));
    }

    [Theory]
    [InlineData(nameof(ComplexityGuardOptions.MaxParserDepth), "maxParserDepth")]
    [InlineData(nameof(ComplexityGuardOptions.MaxInputBytes), "maxInputBytes")]
    [InlineData(nameof(ComplexityGuardOptions.MaxTokens), "maxTokens")]
    [InlineData(nameof(ComplexityGuardOptions.MaxAstNodes), "maxAstNodes")]
    [InlineData(nameof(ComplexityGuardOptions.MaxAstDepth), "maxAstDepth")]
    [InlineData(nameof(ComplexityGuardOptions.MaxParenthesisDepth), "maxParenthesisDepth")]
    [InlineData(nameof(ComplexityGuardOptions.MaxFunctionCallDepth), "maxFunctionCallDepth")]
    public void TestPropertySerialization(string propertyName, string jsonName)
    {
        var options = new ComplexityGuardOptions();
        typeof(ComplexityGuardOptions).GetProperty(propertyName)!.SetValue(options, 42L);

        Assert.Equal($"{{\"{jsonName}\":42}}", Serialize(options));

        var roundTrip = JsonSerializer.Deserialize(Serialize(options), PolyglotJsonContext.Default.ComplexityGuardOptions);
        Assert.Equal(options, roundTrip);
    }

    [Fact]
    public void TestLargeValue()
    {
        var options = new ComplexityGuardOptions { MaxInputBytes = 16L * 1024 * 1024 * 1024 };

        Assert.Equal("{\"maxInputBytes\":17179869184}", Serialize(options));
    }

    [Fact]
    public void TestNestedInTranspileOptions()
    {
        var options = new TranspileOptions { ComplexityGuard = new ComplexityGuardOptions { MaxAstDepth = 10 } };
        string json = JsonSerializer.Serialize(options, PolyglotJsonContext.Default.TranspileOptions);

        Assert.Contains("\"complexityGuard\":{\"maxAstDepth\":10}", json);
    }

    [Fact]
    public void TestOmittedFromTranspileOptionsWhenNull()
    {
        string json = JsonSerializer.Serialize(new TranspileOptions(), PolyglotJsonContext.Default.TranspileOptions);

        Assert.DoesNotContain("complexityGuard", json);
    }

    [Fact]
    public void TestNestedInAnalyzeQueryOptions()
    {
        var options = new AnalyzeQueryOptions { ComplexityGuard = new ComplexityGuardOptions { MaxTokens = 100 } };
        string json = JsonSerializer.Serialize(options, PolyglotJsonContext.Default.AnalyzeQueryOptions);

        Assert.Contains("\"complexityGuard\":{\"maxTokens\":100}", json);
    }

    [Fact]
    public void TestOmittedFromAnalyzeQueryOptionsWhenNull()
    {
        string json = JsonSerializer.Serialize(new AnalyzeQueryOptions(), PolyglotJsonContext.Default.AnalyzeQueryOptions);

        Assert.DoesNotContain("complexityGuard", json);
    }
}
