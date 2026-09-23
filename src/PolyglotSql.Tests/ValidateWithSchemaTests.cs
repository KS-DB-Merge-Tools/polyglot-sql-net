using System.Collections.Generic;
using PolyglotSql;
using PolyglotSql.Bundle;
using PolyglotSql.Models;

namespace PolyglotSql.Tests;

public class ValidateWithSchemaTests
{
    static ValidateWithSchemaTests()
    {
        BundleInitializer.Initialize();
    }

    private static ValidationSchema CreateSchema() => new ValidationSchema
    {
        Tables =
        {
            new SchemaTable
            {
                Name = "customers",
                Columns =
                {
                    new SchemaColumn { Name = "id", Type = "int", PrimaryKey = true },
                    new SchemaColumn { Name = "name", Type = "varchar" },
                },
                PrimaryKey = new List<string> { "id" },
            },
            new SchemaTable
            {
                Name = "orders",
                Columns =
                {
                    new SchemaColumn { Name = "id", Type = "int", PrimaryKey = true },
                    new SchemaColumn
                    {
                        Name = "customer_id",
                        Type = "int",
                        References = new SchemaColumnReference { Table = "customers", Column = "id" },
                    },
                },
                ForeignKeys = new List<SchemaForeignKey>
                {
                    new SchemaForeignKey
                    {
                        Name = "fk_orders_customers",
                        Columns = { "customer_id" },
                        References = new SchemaTableReference { Table = "customers", Columns = { "id" } },
                    },
                },
            },
        },
    };

    [Fact]
    public void TestValidateWithSchemaValid()
    {
        var result = Polyglot.ValidateWithSchema("SELECT id, name FROM customers", CreateSchema());

        Assert.True(result.Valid);
    }

    [Fact]
    public void TestValidateWithSchemaUnknownColumn()
    {
        var result = Polyglot.ValidateWithSchema("SELECT missing FROM customers", CreateSchema());

        Assert.False(result.Valid);
        Assert.Contains(result.Errors, e => e.Code == "E201");
    }

    [Fact]
    public void TestValidateWithSchemaUnknownTable()
    {
        var result = Polyglot.ValidateWithSchema("SELECT id FROM missing", CreateSchema(), Dialect.Generic, new SchemaValidationOptions { Strict = true });

        Assert.False(result.Valid);
        Assert.Contains(result.Errors, e => e.Code == "E200");
    }

    [Fact]
    public void TestValidateWithSchemaAllOptions()
    {
        // Every option is serialized; the native side rejects unknown option names.
        var options = new SchemaValidationOptions
        {
            CheckTypes = true,
            CheckReferences = true,
            Strict = true,
            Semantic = true,
            StrictSyntax = true,
            ComplexityGuard = new ComplexityGuardOptions { MaxAstDepth = 256 },
        };
        string sql = "SELECT o.id, c.name FROM orders o JOIN customers c ON o.customer_id = c.id ORDER BY o.id";
        var result = Polyglot.ValidateWithSchema(sql, CreateSchema(), Dialect.Generic, options);

        Assert.True(result.Valid);
    }

    [Fact]
    public void TestValidateWithSchemaSemanticWarning()
    {
        var result = Polyglot.ValidateWithSchema("SELECT * FROM customers", CreateSchema(), Dialect.Generic, new SchemaValidationOptions { Semantic = true });

        Assert.Contains(result.Errors, e => e.Code == "W001");
    }

    [Fact]
    public void TestValidateWithSchemaGuardExceeded()
    {
        var options = new SchemaValidationOptions { ComplexityGuard = new ComplexityGuardOptions { MaxInputBytes = 5 } };
        var result = Polyglot.ValidateWithSchema("SELECT id FROM customers", CreateSchema(), Dialect.Generic, options);

        Assert.False(result.Valid);
        Assert.Contains(result.Errors, e => e.Message.Contains("E_GUARD_INPUT_TOO_LARGE"));
    }
}
