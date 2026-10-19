using System;
using System.Text.Json;
using PolyglotSql;
using PolyglotSql.Bundle;
using PolyglotSql.Models;

namespace PolyglotSql.Tests;

// Pure serialization tests - no native library required
public class DataTypeHanaVerticaSerializationTests
{
    private static DataType RoundTrip(string json)
    {
        var dataType = JsonSerializer.Deserialize(json, PolyglotJsonContext.Default.DataType);
        Assert.Equal(json, JsonSerializer.Serialize(dataType, PolyglotJsonContext.Default.DataType));
        return dataType;
    }

    [Fact]
    public void TestHanaRoundTrip()
    {
        var hana = Assert.IsType<DataType.Hana>(RoundTrip(
            "{\"data_type\":\"hana\",\"hana_type\":{\"name\":\"DECIMAL\",\"parameters\":[10,2]}}"));
        Assert.Equal("DECIMAL", hana.HanaType.Name);
        Assert.Equal(new uint[] { 10, 2 }, hana.HanaType.Parameters);
    }

    [Fact]
    public void TestHanaEmptyParametersAreWritten()
    {
        var hana = Assert.IsType<DataType.Hana>(RoundTrip(
            "{\"data_type\":\"hana\",\"hana_type\":{\"name\":\"SECONDDATE\",\"parameters\":[]}}"));
        Assert.Equal("SECONDDATE", hana.HanaType.Name);
        Assert.Empty(hana.HanaType.Parameters);

        // parameters is required by native, so a new instance must write it as []
        string json = JsonSerializer.Serialize<DataType>(
            new DataType.Hana { HanaType = new DataType.HanaDataType { Name = "SECONDDATE" } },
            PolyglotJsonContext.Default.DataType);
        Assert.Equal("{\"data_type\":\"hana\",\"hana_type\":{\"name\":\"SECONDDATE\",\"parameters\":[]}}", json);
    }

    [Fact]
    public void TestVerticaArrayWithElementsBound()
    {
        var vertica = Assert.IsType<DataType.Vertica>(RoundTrip(
            "{\"data_type\":\"vertica\",\"vertica_type\":{\"kind\":\"array\",\"element_type\":{\"data_type\":\"int\"},\"bound\":{\"kind\":\"elements\",\"value\":5}}}"));
        var array = Assert.IsType<DataType.VerticaArray>(vertica.VerticaType);
        Assert.IsType<DataType.Int>(array.ElementType);
        var bound = Assert.IsType<DataType.VerticaBoundElements>(array.Bound);
        Assert.Equal(5u, bound.Value);
    }

    [Fact]
    public void TestVerticaArrayWithoutBound()
    {
        var vertica = Assert.IsType<DataType.Vertica>(RoundTrip(
            "{\"data_type\":\"vertica\",\"vertica_type\":{\"kind\":\"array\",\"element_type\":{\"data_type\":\"int\"}}}"));
        var array = Assert.IsType<DataType.VerticaArray>(vertica.VerticaType);
        Assert.Null(array.Bound);
    }

    [Fact]
    public void TestVerticaSetWithBytesBound()
    {
        var vertica = Assert.IsType<DataType.Vertica>(RoundTrip(
            "{\"data_type\":\"vertica\",\"vertica_type\":{\"kind\":\"set\",\"element_type\":{\"data_type\":\"var_char\",\"length\":10},\"bound\":{\"kind\":\"bytes\",\"value\":100}}}"));
        var set = Assert.IsType<DataType.VerticaSet>(vertica.VerticaType);
        var element = Assert.IsType<DataType.VarChar>(set.ElementType);
        Assert.Equal(10u, element.Length);
        var bound = Assert.IsType<DataType.VerticaBoundBytes>(set.Bound);
        Assert.Equal(100u, bound.Value);
    }

    [Fact]
    public void TestVerticaRow()
    {
        var vertica = Assert.IsType<DataType.Vertica>(RoundTrip(
            "{\"data_type\":\"vertica\",\"vertica_type\":{\"kind\":\"row\",\"fields\":[{\"name\":\"a\",\"data_type\":{\"data_type\":\"int\"}},{\"name\":\"b\",\"data_type\":{\"data_type\":\"text\"}}]}}"));
        var row = Assert.IsType<DataType.VerticaRow>(vertica.VerticaType);
        Assert.Equal(2, row.Fields.Count);
        Assert.Equal("a", row.Fields[0].Name);
        Assert.IsType<DataType.Int>(row.Fields[0].Type);
        Assert.Equal("b", row.Fields[1].Name);
        Assert.IsType<DataType.Text>(row.Fields[1].Type);
    }

    [Fact]
    public void TestVerticaLongVarBinary()
    {
        var vertica = Assert.IsType<DataType.Vertica>(RoundTrip(
            "{\"data_type\":\"vertica\",\"vertica_type\":{\"kind\":\"long_var_binary\",\"length\":100}}"));
        Assert.Equal(100u, Assert.IsType<DataType.VerticaLongVarBinary>(vertica.VerticaType).Length);

        vertica = Assert.IsType<DataType.Vertica>(RoundTrip(
            "{\"data_type\":\"vertica\",\"vertica_type\":{\"kind\":\"long_var_binary\"}}"));
        Assert.Null(Assert.IsType<DataType.VerticaLongVarBinary>(vertica.VerticaType).Length);
    }

    [Fact]
    public void TestVerticaIntervalAllFields()
    {
        var vertica = Assert.IsType<DataType.Vertica>(RoundTrip(
            "{\"data_type\":\"vertica\",\"vertica_type\":{\"kind\":\"interval\",\"precision\":3,\"year_month\":false,\"unit\":\"DAY\",\"end_unit\":\"SECOND\"}}"));
        var interval = Assert.IsType<DataType.VerticaInterval>(vertica.VerticaType);
        Assert.Equal(3u, interval.Precision);
        Assert.False(interval.YearMonth);
        Assert.Equal("DAY", interval.Unit);
        Assert.Equal("SECOND", interval.EndUnit);
    }

    [Fact]
    public void TestVerticaIntervalMinimal()
    {
        var vertica = Assert.IsType<DataType.Vertica>(RoundTrip(
            "{\"data_type\":\"vertica\",\"vertica_type\":{\"kind\":\"interval\",\"year_month\":true}}"));
        var interval = Assert.IsType<DataType.VerticaInterval>(vertica.VerticaType);
        Assert.True(interval.YearMonth);
        Assert.Null(interval.Precision);
        Assert.Null(interval.Unit);
        Assert.Null(interval.EndUnit);

        // year_month is required by native, so false must be written too
        string json = JsonSerializer.Serialize<DataType>(
            new DataType.Vertica { VerticaType = new DataType.VerticaInterval() },
            PolyglotJsonContext.Default.DataType);
        Assert.Equal("{\"data_type\":\"vertica\",\"vertica_type\":{\"kind\":\"interval\",\"year_month\":false}}", json);
    }
}

public class DataTypeHanaVerticaParseTests
{
    static DataTypeHanaVerticaParseTests()
    {
        BundleInitializer.Initialize();
    }

    [Theory]
    [InlineData("ALPHANUM(10)", "ALPHANUM", new uint[] { 10 })]
    [InlineData("SHORTTEXT(100)", "SHORTTEXT", new uint[] { 100 })]
    [InlineData("SECONDDATE", "SECONDDATE", new uint[0])]
    [InlineData("ST_POINT(4326)", "ST_POINT", new uint[] { 4326 })]
    public void TestParseHana(string sql, string name, uint[] parameters)
    {
        var dataType = Polyglot.ParseDataType(sql, Dialect.HANA);
        Console.WriteLine($"Result: {JsonSerializer.Serialize(dataType, PolyglotJsonContext.Default.DataType)}");

        var hana = Assert.IsType<DataType.Hana>(dataType);
        Assert.Equal(name, hana.HanaType.Name);
        Assert.Equal(parameters, hana.HanaType.Parameters);
        Assert.Equal(sql, Polyglot.GenerateDataType(dataType, Dialect.HANA));
    }

    [Fact]
    public void TestParseVerticaArrayWithElementsBound()
    {
        // Vertica INT is a 64-bit integer, so it parses as BigInt and generates back as BIGINT
        var dataType = Polyglot.ParseDataType("ARRAY[INT, 5]", Dialect.Vertica);
        Console.WriteLine($"Result: {JsonSerializer.Serialize(dataType, PolyglotJsonContext.Default.DataType)}");

        var array = Assert.IsType<DataType.VerticaArray>(Assert.IsType<DataType.Vertica>(dataType).VerticaType);
        Assert.IsType<DataType.BigInt>(array.ElementType);
        Assert.Equal(5u, Assert.IsType<DataType.VerticaBoundElements>(array.Bound).Value);
        Assert.Equal("ARRAY[BIGINT, 5]", Polyglot.GenerateDataType(dataType, Dialect.Vertica));
    }

    [Fact]
    public void TestParseVerticaSetWithBytesBound()
    {
        var dataType = Polyglot.ParseDataType("SET[VARCHAR(10)](100)", Dialect.Vertica);
        Console.WriteLine($"Result: {JsonSerializer.Serialize(dataType, PolyglotJsonContext.Default.DataType)}");

        var set = Assert.IsType<DataType.VerticaSet>(Assert.IsType<DataType.Vertica>(dataType).VerticaType);
        Assert.Equal(10u, Assert.IsType<DataType.VarChar>(set.ElementType).Length);
        Assert.Equal(100u, Assert.IsType<DataType.VerticaBoundBytes>(set.Bound).Value);
        Assert.Equal("SET[VARCHAR(10)](100)", Polyglot.GenerateDataType(dataType, Dialect.Vertica));
    }

    [Fact]
    public void TestParseVerticaRow()
    {
        // Vertica INT is a 64-bit integer, so it parses as BigInt and generates back as BIGINT
        var dataType = Polyglot.ParseDataType("ROW(a INT, b VARCHAR(10))", Dialect.Vertica);
        Console.WriteLine($"Result: {JsonSerializer.Serialize(dataType, PolyglotJsonContext.Default.DataType)}");

        var row = Assert.IsType<DataType.VerticaRow>(Assert.IsType<DataType.Vertica>(dataType).VerticaType);
        Assert.Equal(2, row.Fields.Count);
        Assert.Equal("a", row.Fields[0].Name);
        Assert.IsType<DataType.BigInt>(row.Fields[0].Type);
        Assert.Equal("b", row.Fields[1].Name);
        Assert.Equal(10u, Assert.IsType<DataType.VarChar>(row.Fields[1].Type).Length);
        Assert.Equal("ROW(a BIGINT, b VARCHAR(10))", Polyglot.GenerateDataType(dataType, Dialect.Vertica));
    }

    [Fact]
    public void TestParseVerticaLongVarBinary()
    {
        var dataType = Polyglot.ParseDataType("LONG VARBINARY(100)", Dialect.Vertica);
        Console.WriteLine($"Result: {JsonSerializer.Serialize(dataType, PolyglotJsonContext.Default.DataType)}");

        var binary = Assert.IsType<DataType.VerticaLongVarBinary>(Assert.IsType<DataType.Vertica>(dataType).VerticaType);
        Assert.Equal(100u, binary.Length);
        Assert.Equal("LONG VARBINARY(100)", Polyglot.GenerateDataType(dataType, Dialect.Vertica));
    }

    [Fact]
    public void TestParseVerticaInterval()
    {
        var dataType = Polyglot.ParseDataType("INTERVAL DAY TO SECOND", Dialect.Vertica);
        Console.WriteLine($"Result: {JsonSerializer.Serialize(dataType, PolyglotJsonContext.Default.DataType)}");

        var interval = Assert.IsType<DataType.VerticaInterval>(Assert.IsType<DataType.Vertica>(dataType).VerticaType);
        Assert.False(interval.YearMonth);
        Assert.Equal("DAY", interval.Unit);
        Assert.Equal("SECOND", interval.EndUnit);
        Assert.Equal("INTERVAL DAY TO SECOND", Polyglot.GenerateDataType(dataType, Dialect.Vertica));
    }

    [Fact]
    public void TestParseVerticaIntervalYearMonth()
    {
        var dataType = Polyglot.ParseDataType("INTERVALYM", Dialect.Vertica);
        Console.WriteLine($"Result: {JsonSerializer.Serialize(dataType, PolyglotJsonContext.Default.DataType)}");

        var interval = Assert.IsType<DataType.VerticaInterval>(Assert.IsType<DataType.Vertica>(dataType).VerticaType);
        Assert.True(interval.YearMonth);
        Assert.Equal("INTERVALYM", Polyglot.GenerateDataType(dataType, Dialect.Vertica));
    }

    [Theory]
    [InlineData(Dialect.HANA)]
    [InlineData(Dialect.Vertica)]
    public void TestTranspileSmoke(Dialect dialect)
    {
        string[] toDialect = Polyglot.Transpile("SELECT a, b FROM t WHERE a > 1", Dialect.Generic, dialect);
        Assert.Equal("SELECT a, b FROM t WHERE a > 1", Assert.Single(toDialect));

        string[] fromDialect = Polyglot.Transpile("SELECT a, b FROM t WHERE a > 1", dialect, Dialect.Generic);
        Assert.Equal("SELECT a, b FROM t WHERE a > 1", Assert.Single(fromDialect));
    }
}
