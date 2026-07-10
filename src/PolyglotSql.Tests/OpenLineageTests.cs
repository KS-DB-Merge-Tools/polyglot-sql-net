using System;
using PolyglotSql;
using PolyglotSql.Bundle;
using PolyglotSql.Models;

namespace PolyglotSql.Tests;

public class OpenLineageTests
{
    static OpenLineageTests()
    {
        BundleInitializer.Initialize();
    }

    private static OpenLineageOptions BasicOptions()
    {
        return new OpenLineageOptions
        {
            Dialect = Dialect.PostgreSQL,
            Producer = "https://github.com/tobilg/polyglot",
            DatasetNamespace = "postgres://warehouse",
            OutputDataset = new OpenLineageDatasetId { Namespace = "postgres://warehouse", Name = "analytics.out" },
            JobNamespace = "polyglot-tests",
            JobName = "lineage-test",
            EventTime = "2026-05-18T00:00:00Z",
            RunId = "3b452093-782c-4ef2-9c0c-aafe2aa6f34d",
            EventType = OpenLineageRunEventType.COMPLETE,
        };
    }

    [Fact]
    public void TestOpenLineageColumnLineage()
    {
        Console.WriteLine("=== TestOpenLineageColumnLineage ===");
        try
        {
            string sql = "SELECT a FROM t";
            var options = BasicOptions();
            var result = Polyglot.OpenLineageColumnLineage(sql, options);
            Console.WriteLine($"Output: {result.Outputs?.Length ?? 0} dataset(s), field 'a' inputs: {result.Facet?.Fields["a"].InputFields.Length}");

            Assert.NotNull(result);
            Assert.NotNull(result.Facet);
            Assert.NotNull(result.Facet.Fields);
            Assert.True(result.Facet.Fields.ContainsKey("a"));
            Assert.Contains(result.Facet.Fields["a"].InputFields, f => f.Name == "t" && f.Field == "a");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: {ex.GetType().Name}: {ex.Message}");
            throw;
        }
    }

    [Fact]
    public void TestOpenLineageJobEvent()
    {
        Console.WriteLine("=== TestOpenLineageJobEvent ===");
        try
        {
            string sql = "SELECT a FROM t";
            var options = BasicOptions();
            var result = Polyglot.OpenLineageJobEvent(sql, options);

            Assert.NotNull(result);
            Assert.True(result.Event.ValueKind == System.Text.Json.JsonValueKind.Object);
            Assert.Equal("polyglot-tests", result.Event.GetProperty("job").GetProperty("namespace").GetString());
            Assert.True(result.Event.TryGetProperty("outputs", out var outputs) && outputs.GetArrayLength() > 0);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: {ex.GetType().Name}: {ex.Message}");
            throw;
        }
    }

    [Fact]
    public void TestOpenLineageRunEvent()
    {
        Console.WriteLine("=== TestOpenLineageRunEvent ===");
        try
        {
            string sql = "SELECT a FROM t";
            var options = BasicOptions();
            var result = Polyglot.OpenLineageRunEvent(sql, options);

            Assert.NotNull(result);
            Assert.Equal("COMPLETE", result.Event.GetProperty("eventType").GetString());
            Assert.Equal("3b452093-782c-4ef2-9c0c-aafe2aa6f34d", result.Event.GetProperty("run").GetProperty("runId").GetString());
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: {ex.GetType().Name}: {ex.Message}");
            throw;
        }
    }
}
