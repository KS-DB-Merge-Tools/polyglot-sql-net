using PolyglotSql;
using PolyglotSql.Bundle;
using PolyglotSql.Models;

namespace PolyglotSql.Tests;

public class OutputColumnsWithSchemaTests
{
    static OutputColumnsWithSchemaTests()
    {
        BundleInitializer.Initialize();
    }

    [Fact]
    public void TestOutputColumnsWithSchemaExpandsWildcard()
    {
        var schema = new ValidationSchema
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

        var output = Polyglot.OutputColumnsWithSchema("SELECT * FROM t", schema);

        Assert.True(output.OrdinalComplete);
        Assert.Equal(2, output.Columns.Count);
        var a = Assert.IsType<OutputColumn.Named>(output.Columns[0]);
        Assert.Equal("a", a.Name);
        Assert.Equal(0, a.Ordinal);
        var b = Assert.IsType<OutputColumn.Named>(output.Columns[1]);
        Assert.Equal("b", b.Name);
        Assert.Equal(1, b.Ordinal);
    }

    [Fact]
    public void TestOutputColumnsWithSchemaUnknownTableKeepsWildcard()
    {
        var schema = new ValidationSchema();

        var output = Polyglot.OutputColumnsWithSchema("SELECT * FROM t", schema);

        Assert.False(output.OrdinalComplete);
        Assert.IsType<OutputColumn.Wildcard>(Assert.Single(output.Columns));
    }
}
