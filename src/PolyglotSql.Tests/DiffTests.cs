using PolyglotSql;
using PolyglotSql.Bundle;
using PolyglotSql.Models;

namespace PolyglotSql.Tests;

public class DiffTests
{
    static DiffTests()
    {
        BundleInitializer.Initialize();
    }

    [Fact]
    public void TestDiffIdentical()
    {
        string sql = "SELECT 1";
        var result = Polyglot.Diff(sql, sql);
        Console.WriteLine($"Diff result: {result}");

        Assert.True(result.AreEqual, "Identical SQL should have no differences");
    }

    [Fact]
    public void TestDiffDifferentColumns()
    {
        string sql1 = "SELECT a FROM t";
        string sql2 = "SELECT b FROM t";
        var result = Polyglot.Diff(sql1, sql2);
        Console.WriteLine($"SQL1: {sql1}");
        Console.WriteLine($"SQL2: {sql2}");
        Console.WriteLine($"Diff result: {result}");
        foreach (var edit in result.Edits)
        {
            Console.WriteLine($"  Edit: {edit}");
        }

        Assert.False(result.AreEqual, "Different SQL should have differences");
    }

    [Fact]
    public void TestDiffAddedWhere()
    {
        string sql1 = "SELECT * FROM users";
        string sql2 = "SELECT * FROM users WHERE id = 1";
        var result = Polyglot.Diff(sql1, sql2);
        Console.WriteLine($"SQL1: {sql1}");
        Console.WriteLine($"SQL2: {sql2}");
        Console.WriteLine($"Diff result: {result}");
        foreach (var edit in result.Edits)
        {
            Console.WriteLine($"  Edit: {edit}");
        }

        Assert.True(result.InsertCount > 0 || result.UpdateCount > 0, "Should have inserts or updates for added WHERE clause");
    }

    [Fact]
    public void TestDiffComplexQueries()
    {
        string sql1 = "SELECT a, b FROM t WHERE c = 1";
        string sql2 = "SELECT a, b, c FROM t WHERE c = 1 AND d = 2";
        var result = Polyglot.Diff(sql1, sql2);
        Console.WriteLine($"SQL1: {sql1}");
        Console.WriteLine($"SQL2: {sql2}");
        Console.WriteLine($"Diff result: {result}");
        foreach (var edit in result.Edits)
        {
            Console.WriteLine($"  Edit: {edit}");
        }

        Assert.False(result.AreEqual, "Different SQL should have differences");
    }

    [Fact]
    public void TestDiffWithDialect()
    {
        string sql1 = "SELECT * FROM users LIMIT 10";
        string sql2 = "SELECT * FROM users LIMIT 20";
        var result = Polyglot.Diff(sql1, sql2, Dialect.MySQL);
        Console.WriteLine($"SQL1: {sql1}");
        Console.WriteLine($"SQL2: {sql2}");
        Console.WriteLine($"Dialect: mysql");
        Console.WriteLine($"Diff result: {result}");
        foreach (var edit in result.Edits)
        {
            Console.WriteLine($"  Edit: {edit}");
        }
    }
}