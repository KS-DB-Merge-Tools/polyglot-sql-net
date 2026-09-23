using PolyglotSql;
using PolyglotSql.Bundle;
using PolyglotSql.Models;

namespace PolyglotSql.Tests;

public class LineageSetBranchTests
{
    static LineageSetBranchTests()
    {
        BundleInitializer.Initialize();
    }

    [Fact]
    public void TestLineageUnionAllBranches()
    {
        var node = Polyglot.Lineage("a", "SELECT a FROM t UNION ALL SELECT b FROM u");
        Console.WriteLine($"Lineage: {node}");

        Assert.Null(node.SetBranch);
        Assert.Equal(2, node.Downstream.Count);

        for (int i = 0; i < 2; i++)
        {
            var branch = node.Downstream[i].SetBranch;
            Assert.NotNull(branch);
            Assert.Equal(SetOperator.union, branch.Operator);
            Assert.Equal(i, branch.Ordinal);
            Assert.True(branch.All);
        }
    }

    [Fact]
    public void TestLineageExceptBranches()
    {
        var node = Polyglot.Lineage("a", "SELECT a FROM t EXCEPT SELECT b FROM u");

        Assert.Contains(node.Downstream, d => d.SetBranch != null && d.SetBranch.Operator == SetOperator.except && !d.SetBranch.All);
    }

    [Fact]
    public void TestLineageNoSetOperation()
    {
        var node = Polyglot.Lineage("a", "SELECT a FROM t");

        Assert.Null(node.SetBranch);
        Assert.All(node.Downstream, d => Assert.Null(d.SetBranch));
    }
}
