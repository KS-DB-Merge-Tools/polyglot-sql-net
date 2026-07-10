using System.Collections.Generic;
using PolyglotSql;
using PolyglotSql.Bundle;
using PolyglotSql.Models;

namespace PolyglotSql.Tests;

public class TableTests
{
    static TableTests()
    {
        BundleInitializer.Initialize();
    }

    [Fact]
    public void TestSourceTablesSimple()
    {
        Console.WriteLine("=== TestSourceTablesSimple ===");
        try
        {
            string sql = "SELECT a FROM t";
            var tables = Polyglot.SourceTables("a", sql);
            Console.WriteLine($"SQL: {sql}");
            Console.WriteLine($"Source tables: {string.Join(", ", tables)}");

            Assert.NotNull(tables);
            Assert.Contains("t", tables);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: {ex.GetType().Name}: {ex.Message}");
            throw;
        }
    }

    [Fact]
    public void TestSourceTablesDerivedColumn()
    {
        Console.WriteLine("=== TestSourceTablesDerivedColumn ===");
        try
        {
            string sql = "SELECT a + b AS c FROM t";
            var tables = Polyglot.SourceTables("c", sql);
            Console.WriteLine($"SQL: {sql}");
            Console.WriteLine($"Source tables: {string.Join(", ", tables)}");

            Assert.NotNull(tables);
            Assert.Contains("t", tables);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: {ex.GetType().Name}: {ex.Message}");
            throw;
        }
    }

    [Fact]
    public void TestQualifyTables()
    {
        Console.WriteLine("=== TestQualifyTables ===");
        try
        {
            string sql = "SELECT a FROM t";
            var ast = Polyglot.Parse(sql);
            var options = new QualifyTablesOptions { Db = "mydb" };
            var result = Polyglot.QualifyTables(ast, options);

            Console.WriteLine($"SQL: {sql}");
            Console.WriteLine($"Qualified AST: {result[0].ToJsonString()}");

            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Contains("mydb", result[0].ToJsonString());
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: {ex.GetType().Name}: {ex.Message}");
            throw;
        }
    }

    [Fact]
    public void TestRenameTables()
    {
        Console.WriteLine("=== TestRenameTables ===");
        try
        {
            string sql = "SELECT a FROM t";
            var ast = Polyglot.Parse(sql);
            var mapping = new Dictionary<string, string> { ["t"] = "t2" };
            var options = new RenameTablesOptions();
            var result = Polyglot.RenameTablesWithOptions(ast, mapping, options);

            Console.WriteLine($"SQL: {sql}");
            Console.WriteLine($"Renamed AST: {result[0].ToJsonString()}");

            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Contains("t2", result[0].ToJsonString());
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: {ex.GetType().Name}: {ex.Message}");
            throw;
        }
    }
}
