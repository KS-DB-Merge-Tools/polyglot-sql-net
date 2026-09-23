using System.Text.Json;
using PolyglotSql;
using PolyglotSql.Models;

namespace PolyglotSql.Tests;

// Pure serialization tests - no native library required
public class ParseOptionsTests
{
    [Fact]
    public void TestDefaultSerializesToEmptyObject()
    {
        Assert.Equal("{}", JsonSerializer.Serialize(new ParseOptions(), PolyglotJsonContext.Default.ParseOptions));
    }

    [Fact]
    public void TestComplexityGuard()
    {
        var options = new ParseOptions { ComplexityGuard = new ComplexityGuardOptions { MaxFunctionCallDepth = 128 } };

        Assert.Equal("{\"complexityGuard\":{\"maxFunctionCallDepth\":128}}",
            JsonSerializer.Serialize(options, PolyglotJsonContext.Default.ParseOptions));
    }
}
