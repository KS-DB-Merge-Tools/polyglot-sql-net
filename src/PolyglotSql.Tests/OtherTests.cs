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
        Console.WriteLine("=== TestAnalyzeQuery ===");
        try
        {
            string sql = "SELECT a FROM t";
            var analysis = Polyglot.AnalyzeQuery(sql);
            Console.WriteLine($"Shape: {analysis.Shape}, projections: {analysis.Projections.Length}, relations: {analysis.Relations.Length}");

            Assert.Equal(QueryShape.select, analysis.Shape);
            Assert.True(analysis.Projections.Length >= 1);
            Assert.Contains(analysis.Relations, r => r.Name == "t");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: {ex.GetType().Name}: {ex.Message}");
            throw;
        }
    }

    [Fact]
    public void TestValidateValid()
    {
        Console.WriteLine("=== TestValidateValid ===");
        try
        {
            var result = Polyglot.Validate("SELECT 1");
            Console.WriteLine($"Valid: {result.Valid}, errors: {result.Errors?.Length ?? 0}");

            Assert.True(result.Valid, "SELECT 1 should be valid");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: {ex.GetType().Name}: {ex.Message}");
            throw;
        }
    }

    [Fact]
    public void TestValidateInvalid()
    {
        Console.WriteLine("=== TestValidateInvalid ===");
        try
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
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: {ex.GetType().Name}: {ex.Message}");
            throw;
        }
    }
}
