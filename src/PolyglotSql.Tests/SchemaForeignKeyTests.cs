using System.Collections.Generic;
using System.Text.Json;
using PolyglotSql;
using PolyglotSql.Models;

namespace PolyglotSql.Tests;

// Pure serialization tests - no native library required
public class SchemaForeignKeyTests
{
    [Fact]
    public void TestForeignKeySerialization()
    {
        var fk = new SchemaForeignKey
        {
            Name = "fk_orders_customers",
            Columns = { "customer_id" },
            References = new SchemaTableReference { Table = "customers", Columns = { "id" }, Schema = "sales" },
        };
        string json = JsonSerializer.Serialize(fk, PolyglotJsonContext.Default.SchemaForeignKey);

        Assert.Equal("{\"name\":\"fk_orders_customers\",\"columns\":[\"customer_id\"],"
            + "\"references\":{\"table\":\"customers\",\"columns\":[\"id\"],\"schema\":\"sales\"}}", json);
    }

    [Fact]
    public void TestForeignKeyOptionalFieldsOmitted()
    {
        var fk = new SchemaForeignKey
        {
            Columns = { "customer_id" },
            References = new SchemaTableReference { Table = "customers", Columns = { "id" } },
        };
        string json = JsonSerializer.Serialize(fk, PolyglotJsonContext.Default.SchemaForeignKey);

        Assert.Equal("{\"columns\":[\"customer_id\"],\"references\":{\"table\":\"customers\",\"columns\":[\"id\"]}}", json);
    }

    [Fact]
    public void TestForeignKeyRoundTrip()
    {
        string json = "{\"name\":\"fk\",\"columns\":[\"a\",\"b\"],\"references\":{\"table\":\"p\",\"columns\":[\"x\",\"y\"],\"schema\":\"s\"}}";
        var fk = JsonSerializer.Deserialize(json, PolyglotJsonContext.Default.SchemaForeignKey)!;

        Assert.Equal("fk", fk.Name);
        Assert.Equal(new[] { "a", "b" }, fk.Columns);
        Assert.Equal("p", fk.References.Table);
        Assert.Equal(new[] { "x", "y" }, fk.References.Columns);
        Assert.Equal("s", fk.References.Schema);
    }

    [Fact]
    public void TestSchemaTableForeignKeys()
    {
        var table = new SchemaTable
        {
            Name = "orders",
            ForeignKeys = new List<SchemaForeignKey>
            {
                new SchemaForeignKey { Columns = { "c" }, References = new SchemaTableReference { Table = "p", Columns = { "id" } } },
            },
        };
        string json = JsonSerializer.Serialize(table, PolyglotJsonContext.Default.SchemaTable);

        Assert.Contains("\"foreignKeys\":[{\"columns\":[\"c\"],\"references\":{\"table\":\"p\",\"columns\":[\"id\"]}}]", json);
    }

    [Fact]
    public void TestSchemaTableForeignKeysOmittedWhenNull()
    {
        string json = JsonSerializer.Serialize(new SchemaTable { Name = "t" }, PolyglotJsonContext.Default.SchemaTable);

        Assert.DoesNotContain("foreignKeys", json);
    }

    [Fact]
    public void TestSchemaColumnReferences()
    {
        var column = new SchemaColumn
        {
            Name = "customer_id",
            References = new SchemaColumnReference { Table = "customers", Column = "id", Schema = "sales" },
        };
        string json = JsonSerializer.Serialize(column, PolyglotJsonContext.Default.SchemaColumn);

        Assert.Contains("\"references\":{\"table\":\"customers\",\"column\":\"id\",\"schema\":\"sales\"}", json);
    }

    [Fact]
    public void TestSchemaColumnReferencesOmittedWhenNull()
    {
        string json = JsonSerializer.Serialize(new SchemaColumn { Name = "a" }, PolyglotJsonContext.Default.SchemaColumn);

        Assert.DoesNotContain("references", json);
    }
}
