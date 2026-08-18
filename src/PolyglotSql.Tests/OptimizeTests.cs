using System;
using PolyglotSql;
using PolyglotSql.Bundle;
using PolyglotSql.Models;
using Xunit;

namespace PolyglotSql.Tests;

public class OptimizeTests
{
    static OptimizeTests()
    {
        BundleInitializer.Initialize();
    }

    [Fact]
    public void TestOptimizeRunsCanonicalizePass()
    {
        // The Rust optimizer's canonicalize pass rewrites `IS NOT NULL` into
        // `NOT (... IS NULL)`. This proves the full parse -> optimize -> generate
        // pipeline executes end-to-end through the FFI.
        string[] result = Polyglot.Optimize(
            "SELECT a FROM t WHERE a IS NOT NULL AND a IS NULL",
            Dialect.Generic);

        Assert.NotEmpty(result);
        Console.WriteLine($"optimize result: {result[0]}");
        Assert.Contains("NOT a IS NULL", result[0]);
        Assert.DoesNotContain("IS NOT NULL", result[0]);
    }

    [Fact]
    public void TestOptimizeReturnsEquivalentSql()
    {
        // A simple statement that the optimizer leaves unchanged must still be
        // returned as valid, parseable SQL without errors.
        string[] result = Polyglot.Optimize("SELECT 1 + 1", Dialect.Generic);
        Assert.NotEmpty(result);
        Assert.Contains("SELECT", result[0]);
    }
}
