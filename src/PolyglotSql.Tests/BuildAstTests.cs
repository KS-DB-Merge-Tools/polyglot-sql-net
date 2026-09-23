using PolyglotSql;
using PolyglotSql.Bundle;
using PolyglotSql.Models;

namespace PolyglotSql.Tests;

public class BuildAstTests
{
    static BuildAstTests()
    {
        BundleInitializer.Initialize();
    }

    private const string PlanJson = @"{
        ""base"": { ""kind"": ""select"", ""expressions"": [ { ""kind"": ""column"", ""name"": ""a"" } ] },
        ""operations"": [
            { ""kind"": ""from"", ""source"": { ""kind"": ""table"", ""name"": ""t"" } },
            { ""kind"": ""where"", ""expressions"": [ {
                ""kind"": ""binary"", ""op"": ""gt"",
                ""left"": { ""kind"": ""column"", ""name"": ""a"" },
                ""right"": { ""kind"": ""literal"", ""value"": { ""kind"": ""integer"", ""value"": 1 } }
            } ] }
        ]
    }";

    [Fact]
    public void TestBuildAst()
    {
        var ast = Polyglot.BuildAst(BuilderPlan.FromJson(PlanJson));
        Console.WriteLine($"AST: {ast.ToJsonString()}");

        Assert.NotNull(ast);
        Assert.Equal("SELECT a FROM t WHERE a > 1", Polyglot.GenerateOne(ast));
    }

    [Fact]
    public void TestBuildAstReadDialect()
    {
        // Raw SQL fragments are parsed with the read dialect
        string planJson = @"{ ""base"": { ""kind"": ""select"", ""expressions"": [ { ""kind"": ""sql"", ""sql"": ""[my col]"" } ] } }";
        var ast = Polyglot.BuildAst(BuilderPlan.FromJson(planJson), Dialect.TSQL);

        Assert.Equal("SELECT \"my col\"", Polyglot.GenerateOne(ast));
    }

    [Fact]
    public void TestBuildAstInvalidPlan()
    {
        string planJson = @"{ ""base"": { ""kind"": ""no_such_kind"" } }";

        Assert.Throws<PolyglotException>(() => Polyglot.BuildAst(BuilderPlan.FromJson(planJson)));
    }
}
