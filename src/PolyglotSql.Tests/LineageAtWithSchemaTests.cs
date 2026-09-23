using PolyglotSql;
using PolyglotSql.Bundle;
using PolyglotSql.Models;

namespace PolyglotSql.Tests;

public class LineageAtWithSchemaTests
{
    static LineageAtWithSchemaTests()
    {
        BundleInitializer.Initialize();
    }

    private static ValidationSchema CreateSchema() => new ValidationSchema
    {
        Tables =
        {
            new SchemaTable
            {
                Name = "t",
                Columns =
                {
                    new SchemaColumn { Name = "a", Type = "int" },
                    new SchemaColumn { Name = "b", Type = "varchar" },
                }
            }
        }
    };

    [Fact]
    public void TestLineageAtWithSchemaExpandsWildcard()
    {
        var node = Polyglot.LineageAtWithSchema(1, "SELECT * FROM t", CreateSchema());
        Console.WriteLine($"Lineage: {node}");

        Assert.NotNull(node);
        Assert.Equal("b", node.Name);
    }

    [Fact]
    public void TestLineageAtWithSchemaOrdinalOutOfRange()
    {
        Assert.Throws<PolyglotException>(() => Polyglot.LineageAtWithSchema(2, "SELECT * FROM t", CreateSchema()));
    }

    [Fact]
    public void TestLineageAtWithSchemaNegativeOrdinal()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Polyglot.LineageAtWithSchema(-1, "SELECT * FROM t", CreateSchema()));
    }
}
