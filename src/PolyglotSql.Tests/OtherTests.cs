using PolyglotSql;
using PolyglotSql.Bundle;
using PolyglotSql.Models;

namespace PolyglotSql.Tests;

public class OtherTests
{
    static OtherTests()
    {
        BundleInitializer.Initialize();
    }

    [Fact]
    public void TestAnalyzeQuery()
    {
        string sql = "SELECT a FROM t";
        var analysis = Polyglot.AnalyzeQuery(sql);
        Console.WriteLine($"Shape: {analysis.Shape}, projections: {analysis.Projections.Length}, relations: {analysis.Relations.Length}");

        Assert.Equal(QueryShape.select, analysis.Shape);
        Assert.True(analysis.Projections.Length >= 1);
        Assert.Contains(analysis.Relations, r => r.Name == "t");
    }

    [Fact]
    public void TestValidateValid()
    {
        var result = Polyglot.Validate("SELECT 1");
        Console.WriteLine($"Valid: {result.Valid}, errors: {result.Errors?.Length ?? 0}");

        Assert.True(result.Valid, "SELECT 1 should be valid");
    }

    [Fact]
    public void TestValidateInvalid()
    {
        var result = Polyglot.Validate("SELCT 1");
        Console.WriteLine($"Valid: {result.Valid}, errors: {result.Errors?.Length ?? 0}");
        if (result.Errors != null)
            foreach (var e in result.Errors)
                Console.WriteLine($"  [{e.Code}] {e.Message}");

        Assert.False(result.Valid, "SELCT 1 (typo) should be invalid");
        Assert.NotNull(result.Errors);
        Assert.True(result.Errors.Length > 0);
    }

    [Fact]
    public void TestVersionInfo()
    {
        var versionInfo = Polyglot.VersionInfo;
        Console.WriteLine($"NativeRuntimeVersion: {versionInfo.NativeRuntimeVersion}");
        Console.WriteLine($"WrapperVersion: {versionInfo.WrapperVersion}");
        Console.WriteLine($"NativeExpectedVersion: {versionInfo.NativeExpectedVersion}");

        Assert.False(string.IsNullOrEmpty(versionInfo.NativeRuntimeVersion));
        Assert.StartsWith("0.3.0", versionInfo.WrapperVersion);
    }
}
