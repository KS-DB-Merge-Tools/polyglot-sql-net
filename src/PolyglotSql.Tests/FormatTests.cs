using PolyglotSql;
using PolyglotSql.Bundle;
using PolyglotSql.Models;
using Xunit;

namespace PolyglotSql.Tests;

public class FormatTests
{
    static FormatTests()
    {
        BundleInitializer.Initialize();
    }

    [Fact]
    public void TestFormatBasic()
    {
        Console.WriteLine("=== TestFormatBasic ===");
        string[] result = Polyglot.Format("SELECT 1", Dialect.Generic);
        Assert.NotEmpty(result);
        Assert.Contains("SELECT", result[0]);
        Assert.Contains("\n", result[0]);
    }

    [Fact]
    public void TestFormatWithOptions()
    {
        Console.WriteLine("=== TestFormatWithOptions ===");

        // A limit large enough to allow the input succeeds and returns formatted SQL.
        var allowed = new FormatGuardOptions { MaxInputBytes = 100 };
        string[] ok = Polyglot.FormatWithOptions("SELECT 1", Dialect.Generic, allowed);
        Assert.NotEmpty(ok);
        Assert.Contains("SELECT", ok[0]);

        // A limit smaller than the input size raises a guard error.
        var tooSmall = new FormatGuardOptions { MaxInputBytes = 1 };
        Assert.Throws<PolyglotException>(() =>
            Polyglot.FormatWithOptions("SELECT 1", Dialect.Generic, tooSmall));
    }
}
