using System.Collections.Generic;
using System.Text.Json;
using PolyglotSql;
using PolyglotSql.Bundle;
using PolyglotSql.Models;

namespace PolyglotSql.Tests;

// Pure serialization tests - no native library required
public class StructFieldTests
{
    private const string OptionsJson = "{\"struct_field\":{\"name\":\"x\"}}";

    [Fact]
    public void TestOptionalFieldsOmitted()
    {
        var field = new StructField { Name = "a", Type = new DataType.Int() };
        string json = JsonSerializer.Serialize(field, PolyglotJsonContext.Default.StructField);

        Assert.Equal("{\"name\":\"a\",\"data_type\":{\"data_type\":\"int\"}}", json);
    }

    [Fact]
    public void TestComment()
    {
        var field = new StructField { Name = "a", Type = new DataType.Int(), Comment = "hello" };
        string json = JsonSerializer.Serialize(field, PolyglotJsonContext.Default.StructField);

        Assert.Contains("\"comment\":\"hello\"", json);
        Assert.Equal("hello", JsonSerializer.Deserialize(json, PolyglotJsonContext.Default.StructField)!.Comment);
    }

    [Fact]
    public void TestOptions()
    {
        string json = "{\"name\":\"a\",\"data_type\":{\"data_type\":\"int\"},\"options\":[" + OptionsJson + "]}";
        var field = JsonSerializer.Deserialize(json, PolyglotJsonContext.Default.StructField)!;

        var option = Assert.Single(field.Options);
        Assert.Equal(OptionsJson, option.ToJsonString());
        Assert.Contains("\"options\":[" + OptionsJson + "]", JsonSerializer.Serialize(field, PolyglotJsonContext.Default.StructField));
    }

    [Fact]
    public void TestStructTypeRoundTrip()
    {
        string json = "{\"data_type\":\"struct\",\"fields\":[{\"name\":\"a\",\"data_type\":{\"data_type\":\"int\"},\"comment\":\"c\"}],\"nested\":false}";
        var structType = Assert.IsType<DataType.StructType>(JsonSerializer.Deserialize(json, PolyglotJsonContext.Default.DataType));

        Assert.Equal("c", Assert.Single(structType.Fields).Comment);
        Assert.Equal(json, JsonSerializer.Serialize<DataType>(structType, PolyglotJsonContext.Default.DataType));
    }
}

public class StructFieldParseTests
{
    static StructFieldParseTests()
    {
        BundleInitializer.Initialize();
    }

    [Fact]
    public void TestParseStructFieldOptions()
    {
        string sql = "STRUCT<a INT64 OPTIONS(description = 'x')>";
        var structType = Assert.IsType<DataType.StructType>(Polyglot.ParseDataType(sql, Dialect.BigQuery));

        var field = Assert.Single(structType.Fields);
        Assert.NotNull(field.Options);
        Assert.NotEmpty(field.Options);
        Assert.Contains("OPTIONS", Polyglot.GenerateDataType(structType, Dialect.BigQuery));
    }

    [Fact]
    public void TestParseStructFieldComment()
    {
        string sql = "STRUCT<a: INT COMMENT 'hello'>";
        var structType = Assert.IsType<DataType.StructType>(Polyglot.ParseDataType(sql, Dialect.Spark));

        var field = Assert.Single(structType.Fields);
        Assert.Contains("hello", field.Comment);
        Assert.Contains("COMMENT", Polyglot.GenerateDataType(structType, Dialect.Spark));
    }
}
