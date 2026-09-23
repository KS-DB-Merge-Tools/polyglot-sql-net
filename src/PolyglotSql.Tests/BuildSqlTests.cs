using PolyglotSql;
using PolyglotSql.Bundle;
using PolyglotSql.Models;

namespace PolyglotSql.Tests;

public class BuildSqlTests
{
    static BuildSqlTests()
    {
        BundleInitializer.Initialize();
    }

    private const string PlanJson = @"{
        ""base"": { ""kind"": ""select"", ""expressions"": [ { ""kind"": ""column"", ""name"": ""a"" } ] },
        ""operations"": [
            { ""kind"": ""from"", ""source"": { ""kind"": ""table"", ""name"": ""t"" } },
            { ""kind"": ""limit"", ""expression"": { ""kind"": ""literal"", ""value"": { ""kind"": ""integer"", ""value"": 10 } } }
        ]
    }";

    [Fact]
    public void TestBuildSql()
    {
        string sql = Polyglot.BuildSql(BuilderPlan.FromJson(PlanJson));
        Console.WriteLine($"Result: {sql}");

        Assert.Equal("SELECT a FROM t LIMIT 10", sql);
    }

    [Fact]
    public void TestBuildSqlOutputDialect()
    {
        string sql = Polyglot.BuildSql(BuilderPlan.FromJson(PlanJson), Dialect.Generic, Dialect.TSQL);
        Console.WriteLine($"Result: {sql}");

        Assert.Contains("TOP 10", sql);
    }

    [Fact]
    public void TestBuildSqlRawFragment()
    {
        string planJson = @"{ ""base"": { ""kind"": ""select"", ""expressions"": [ { ""kind"": ""sql"", ""sql"": ""x"" } ] }, ""operations"": [] }";

        Assert.Equal("SELECT x", Polyglot.BuildSql(BuilderPlan.FromJson(planJson)));
    }

    [Fact]
    public void TestBuildSqlInvalidPlan()
    {
        string planJson = @"{ ""base"": { ""kind"": ""select"" } }";

        Assert.Throws<PolyglotException>(() => Polyglot.BuildSql(BuilderPlan.FromJson(planJson)));
    }
}
