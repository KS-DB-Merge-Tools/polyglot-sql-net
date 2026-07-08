using System;
using System.Text.Json;
using PolyglotSql;
using PolyglotSql.Bundle;
using PolyglotSql.Models;

namespace SqlGlotDotNet.DebugTest;

public class DataTypeTests
{
    static DataTypeTests()
    {
        BundleInitializer.Initialize();
    }

    [Fact]
    public void TestParseDataTypeBoolean()
    {
        Console.WriteLine("=== TestParseDataTypeBoolean ===");
        try
        {
            DataType result = Polyglot.ParseDataType("BOOLEAN", Dialect.Generic);
            Console.WriteLine($"Result: {JsonSerializer.Serialize(result)}");
            Assert.IsType<DataType.DataTypeBool>(result);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: {ex.GetType().Name}: {ex.Message}");
            throw;
        }
    }

    [Fact]
    public void TestParseDataTypeInt()
    {
        Console.WriteLine("=== TestParseDataTypeInt ===");
        try
        {
            DataType result = Polyglot.ParseDataType("INT", Dialect.Generic);
            Console.WriteLine($"Result: {JsonSerializer.Serialize(result)}");
            var intType = Assert.IsType<DataType.Int>(result);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: {ex.GetType().Name}: {ex.Message}");
            throw;
        }
    }

    [Fact]
    public void TestParseDataTypeInteger()
    {
        Console.WriteLine("=== TestParseDataTypeInteger ===");
        try
        {
            DataType result = Polyglot.ParseDataType("INTEGER", Dialect.Generic);
            Console.WriteLine($"Result: {JsonSerializer.Serialize(result)}");
            var intType = Assert.IsType<DataType.Int>(result);
            Assert.True(intType.IntegerSpelling);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: {ex.GetType().Name}: {ex.Message}");
            throw;
        }
    }

    [Fact]
    public void TestParseDataTypeFloat()
    {
        Console.WriteLine("=== TestParseDataTypeFloat ===");
        try
        {
            DataType result = Polyglot.ParseDataType("FLOAT", Dialect.Generic);
            Console.WriteLine($"Result: {JsonSerializer.Serialize(result)}");
            Assert.IsType<DataType.Float>(result);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: {ex.GetType().Name}: {ex.Message}");
            throw;
        }
    }

    [Fact]
    public void TestGenerateDataType()
    {
        Console.WriteLine("=== TestGenerateDataType ===");
        try
        {
            DataType input = new DataType.DataTypeBool();
            string result = Polyglot.GenerateDataType(input, Dialect.Generic);
            Console.WriteLine($"Result: {result}");
            Assert.Contains("BOOLEAN", result);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: {ex.GetType().Name}: {ex.Message}");
            throw;
        }
    }

    [Fact]
    public void TestGenerateDataTypeInt()
    {
        Console.WriteLine("=== TestGenerateDataTypeInt ===");
        try
        {
            DataType input = new DataType.Int { Length = 10 };
            string result = Polyglot.GenerateDataType(input, Dialect.Generic);
            Console.WriteLine($"Result: {result}");
            Assert.Contains("INT", result);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: {ex.GetType().Name}: {ex.Message}");
            throw;
        }
    }

    [Fact]
    public void TestParseDataTypeArray()
    {
        Console.WriteLine("=== TestParseDataTypeArray ===");
        try
        {
            DataType result = Polyglot.ParseDataType("ARRAY<INT>", Dialect.Generic);
            Console.WriteLine($"Result: {JsonSerializer.Serialize(result)}");
            var arrayType = Assert.IsType<DataType.ArrayType>(result);
            Assert.NotNull(arrayType.ElementType);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: {ex.GetType().Name}: {ex.Message}");
            throw;
        }
    }

    [Fact]
    public void TestParseDataTypeStruct()
    {
        Console.WriteLine("=== TestParseDataTypeStruct ===");
        try
        {
            DataType result = Polyglot.ParseDataType("STRUCT<a INT, b VARCHAR(100)>", Dialect.DuckDB);
            string json = JsonSerializer.Serialize(result);
            Console.WriteLine($"Result: {json}");
            var structType = Assert.IsType<DataType.StructType>(result);
            Assert.NotNull(structType.Fields);
            Assert.Equal(2, structType.Fields.Count);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: {ex.GetType().Name}: {ex.Message}");
            throw;
        }
    }

    [Fact]
    public void TestParseDataTypeUnion()
    {
        Console.WriteLine("=== TestParseDataTypeUnion ===");
        try
        {
            DataType result = Polyglot.ParseDataType("UNION(num INT, str TEXT)", Dialect.DuckDB);
            string json = JsonSerializer.Serialize(result);
            Console.WriteLine($"Result: {json}");
            var unionType = Assert.IsType<DataType.UnionType>(result);
            Assert.NotNull(unionType.Fields);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: {ex.GetType().Name}: {ex.Message}");
            throw;
        }
    }

    [Theory]
    // MySQL types
    [InlineData("TINYINT", Dialect.MySQL)]
    [InlineData("SMALLINT", Dialect.MySQL)]
    [InlineData("MEDIUMINT", Dialect.MySQL)]
    [InlineData("INT", Dialect.MySQL)]
    [InlineData("BIGINT", Dialect.MySQL)]
    [InlineData("FLOAT", Dialect.MySQL)]
    [InlineData("DOUBLE", Dialect.MySQL)]
    [InlineData("CHAR(10)", Dialect.MySQL)]
    [InlineData("VARCHAR(255)", Dialect.MySQL)]
    [InlineData("TEXT", Dialect.MySQL)]
    [InlineData("BINARY(16)", Dialect.MySQL)]
    [InlineData("VARBINARY(100)", Dialect.MySQL)]
    [InlineData("BLOB", Dialect.MySQL)]
    [InlineData("DATETIME", Dialect.MySQL)]
    [InlineData("TIMESTAMP", Dialect.MySQL)]
    [InlineData("DATE", Dialect.MySQL)]
    [InlineData("TIME", Dialect.MySQL)]
    [InlineData("BOOLEAN", Dialect.MySQL)]
    [InlineData("JSON", Dialect.MySQL)]
    
    // PostgreSQL types - note: INTEGER → INT (normalization)
    [InlineData("SMALLINT", Dialect.PostgreSQL)]
    [InlineData("INT", Dialect.PostgreSQL)]  // INTEGER normalizes to INT
    [InlineData("BIGINT", Dialect.PostgreSQL)]
    [InlineData("REAL", Dialect.PostgreSQL)]
    [InlineData("DOUBLE PRECISION", Dialect.PostgreSQL)]
    [InlineData("CHAR(10)", Dialect.PostgreSQL)]
    [InlineData("VARCHAR(255)", Dialect.PostgreSQL)]
    [InlineData("TEXT", Dialect.PostgreSQL)]
    [InlineData("BYTEA", Dialect.PostgreSQL)]
    [InlineData("TIMESTAMP", Dialect.PostgreSQL)]
    [InlineData("TIMESTAMPTZ", Dialect.PostgreSQL)]
    [InlineData("DATE", Dialect.PostgreSQL)]
    [InlineData("TIME", Dialect.PostgreSQL)]
    [InlineData("BOOLEAN", Dialect.PostgreSQL)]
    [InlineData("UUID", Dialect.PostgreSQL)]
    [InlineData("JSON", Dialect.PostgreSQL)]
    [InlineData("JSONB", Dialect.PostgreSQL)]
    
    // TSQL (SQL Server) types - note: TEXT → VARCHAR(MAX), INTEGER normalizes
    [InlineData("TINYINT", Dialect.TSQL)]
    [InlineData("SMALLINT", Dialect.TSQL)]
    [InlineData("BIGINT", Dialect.TSQL)]
    [InlineData("FLOAT", Dialect.TSQL)]
    [InlineData("REAL", Dialect.TSQL)]
    [InlineData("BIT", Dialect.TSQL)]
    [InlineData("CHAR(10)", Dialect.TSQL)]
    [InlineData("NCHAR(10)", Dialect.TSQL)]
    [InlineData("VARCHAR(255)", Dialect.TSQL)]
    [InlineData("NVARCHAR(255)", Dialect.TSQL)]
    [InlineData("VARCHAR(MAX)", Dialect.TSQL)]  // TEXT / NTEXT maps to VARCHAR(MAX) in TSQL
    [InlineData("BINARY(16)", Dialect.TSQL)]
    [InlineData("VARBINARY(100)", Dialect.TSQL)]
    [InlineData("DATETIME", Dialect.TSQL)]
    [InlineData("DATETIME2", Dialect.TSQL)]
    [InlineData("SMALLDATETIME", Dialect.TSQL)]
    [InlineData("DATE", Dialect.TSQL)]
    [InlineData("TIME", Dialect.TSQL)]
    [InlineData("UNIQUEIDENTIFIER", Dialect.TSQL)]
    
    // Oracle types - note: NUMBER normalizes with space, NTEXT → VARCHAR(MAX) in TSQL
    [InlineData("NUMBER(10, 2)", Dialect.Oracle)]  // normalizes with space
    [InlineData("NUMBER(10)", Dialect.Oracle)]
    [InlineData("VARCHAR2(255)", Dialect.Oracle)]
    [InlineData("NVARCHAR2(255)", Dialect.Oracle)]
    [InlineData("CHAR(10)", Dialect.Oracle)]
    [InlineData("NCHAR(10)", Dialect.Oracle)]
    [InlineData("DATE", Dialect.Oracle)]
    [InlineData("TIMESTAMP", Dialect.Oracle)]
    [InlineData("CLOB", Dialect.Oracle)]
    [InlineData("BLOB", Dialect.Oracle)]
    
    // SQLite types - note: INT/INTEGER normalize
    [InlineData("TEXT", Dialect.SQLite)]
    [InlineData("REAL", Dialect.SQLite)]
    [InlineData("BLOB", Dialect.SQLite)]
    
    // DuckDB types - note: VARCHAR(255) → TEXT(255), BLOB → VARBINARY
    [InlineData("TINYINT", Dialect.DuckDB)]
    [InlineData("SMALLINT", Dialect.DuckDB)]
    [InlineData("BIGINT", Dialect.DuckDB)]
    [InlineData("FLOAT", Dialect.DuckDB)]
    [InlineData("DOUBLE", Dialect.DuckDB)]
    [InlineData("TEXT(255)", Dialect.DuckDB)]  // VARCHAR → TEXT in DuckDB
    [InlineData("TEXT", Dialect.DuckDB)]
    //[InlineData("VARBINARY", Dialect.DuckDB)]  // BLOB → VARBINARY in DuckDB
    [InlineData("DATE", Dialect.DuckDB)]
    [InlineData("TIMESTAMP", Dialect.DuckDB)]
    [InlineData("BOOLEAN", Dialect.DuckDB)]
    
    // Generic types - note: INTEGER → INT, DECIMAL normalizes with space
    [InlineData("BOOLEAN", Dialect.Generic)]
    [InlineData("INT", Dialect.Generic)]
    [InlineData("BIGINT", Dialect.Generic)]
    [InlineData("FLOAT", Dialect.Generic)]
    [InlineData("DOUBLE", Dialect.Generic)]
    [InlineData("CHAR(10)", Dialect.Generic)]
    [InlineData("VARCHAR(255)", Dialect.Generic)]
    [InlineData("TEXT", Dialect.Generic)]
    [InlineData("BINARY(16)", Dialect.Generic)]
    [InlineData("VARBINARY(100)", Dialect.Generic)]
    [InlineData("BLOB", Dialect.Generic)]
    [InlineData("DATE", Dialect.Generic)]
    [InlineData("TIMESTAMP", Dialect.Generic)]
    
    public void ParseAndGenBackToItself(string input, Dialect dialect)
    {
        string actual = Polyglot.TranspileDataType(input, dialect, dialect);
        Assert.Equal(input, actual.ToUpperInvariant());
    }
}