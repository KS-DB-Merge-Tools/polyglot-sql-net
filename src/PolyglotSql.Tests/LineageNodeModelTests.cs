using PolyglotSql;
using PolyglotSql.Models;

namespace PolyglotSql.Tests;

// Pure parsing tests - no native library required
public class LineageNodeModelTests
{
    [Theory]
    [InlineData("union", SetOperator.union)]
    [InlineData("intersect", SetOperator.intersect)]
    [InlineData("except", SetOperator.except)]
    public void TestSetBranch(string op, SetOperator expected)
    {
        string json = @"{ ""name"": ""a"", ""source_name"": """", ""source_kind"": ""root"", ""reference_node_name"": """",
            ""downstream"": [ { ""name"": ""a"", ""source_name"": """", ""source_kind"": ""root"", ""reference_node_name"": """",
                ""downstream"": [], ""set_branch"": { ""operator"": """ + op + @""", ""ordinal"": 1, ""all"": true } } ] }";

        var node = LineageParser.Parse(json);

        Assert.Null(node.SetBranch);
        var branch = Assert.Single(node.Downstream).SetBranch;
        Assert.NotNull(branch);
        Assert.Equal(expected, branch.Operator);
        Assert.Equal(1, branch.Ordinal);
        Assert.True(branch.All);
    }

    [Fact]
    public void TestSetBranchNull()
    {
        var node = LineageParser.Parse(@"{ ""name"": ""a"", ""downstream"": [], ""set_branch"": null }");

        Assert.Null(node.SetBranch);
    }
}
