using System;
using System.Text.Json;
using PolyglotSql;
using PolyglotSql.Bundle;
using PolyglotSql.Models;

namespace PolyglotSql.Tests;

// Pure serialization tests - no native library required
public class DataTypeIntegerTests
{
    [Theory]
    [InlineData(typeof(DataType.Int128Type), "int128")]
    [InlineData(typeof(DataType.UInt8Type), "uint8")]
    [InlineData(typeof(DataType.UInt16Type), "uint16")]
    [InlineData(typeof(DataType.UInt32Type), "uint32")]
    [InlineData(typeof(DataType.UInt64Type), "uint64")]
    [InlineData(typeof(DataType.UInt128Type), "uint128")]
    public void TestSerializationRoundTrip(Type type, string discriminator)
    {
        string json = $"{{\"data_type\":\"{discriminator}\"}}";

        var dataType = JsonSerializer.Deserialize(json, PolyglotJsonContext.Default.DataType);
        Assert.IsType(type, dataType);
        Assert.Equal(json, JsonSerializer.Serialize(dataType, PolyglotJsonContext.Default.DataType));
    }

    [Fact]
    public void TestObsoleteUIntDiscriminatorRejected()
    {
        Assert.Throws<JsonException>(() =>
            JsonSerializer.Deserialize("{\"data_type\":\"u_int\"}", PolyglotJsonContext.Default.DataType));
    }
}

public class DataTypeIntegerParseTests
{
    static DataTypeIntegerParseTests()
    {
        BundleInitializer.Initialize();
    }

    [Theory]
    [InlineData("HUGEINT", typeof(DataType.Int128Type))]
    [InlineData("UTINYINT", typeof(DataType.UInt8Type))]
    [InlineData("USMALLINT", typeof(DataType.UInt16Type))]
    [InlineData("UINTEGER", typeof(DataType.UInt32Type))]
    [InlineData("UBIGINT", typeof(DataType.UInt64Type))]
    [InlineData("UHUGEINT", typeof(DataType.UInt128Type))]
    public void TestParseDuckDB(string sql, Type type)
    {
        var dataType = Polyglot.ParseDataType(sql, Dialect.DuckDB);

        Assert.IsType(type, dataType);
        Assert.Equal(sql, Polyglot.GenerateDataType(dataType, Dialect.DuckDB));
    }

    [Theory]
    [InlineData("Int128", typeof(DataType.Int128Type))]
    [InlineData("UInt8", typeof(DataType.UInt8Type))]
    [InlineData("UInt16", typeof(DataType.UInt16Type))]
    [InlineData("UInt32", typeof(DataType.UInt32Type))]
    [InlineData("UInt64", typeof(DataType.UInt64Type))]
    [InlineData("UInt128", typeof(DataType.UInt128Type))]
    public void TestParseClickHouse(string sql, Type type)
    {
        var dataType = Polyglot.ParseDataType(sql, Dialect.ClickHouse);

        Assert.IsType(type, dataType);
    }
}
