using System;
using System.Text.Json;
using PolyglotSql;
using PolyglotSql.Bundle;

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
            Assert.IsType<DataType.Boolean>(result);
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
            DataType input = new DataType.Boolean();
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
    [InlineData("tinyint", Dialect.TSQL)]
    public void ParseAndGenBackToItself(string type, Dialect dialect)
    {
        string actual = Polyglot.TranspileDataType(type, dialect, dialect);
        Assert.Equal(type.ToUpperInvariant(), actual.ToUpperInvariant());
    }
}