using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

using PolyglotSql.Models;

namespace PolyglotSql
{
    [JsonSerializable(typeof(string[]))]
    [JsonSerializable(typeof(Token[]))]
    [JsonSerializable(typeof(TokenType))]
    [JsonSerializable(typeof(DataType))]
    [JsonSerializable(typeof(StructField))]
    [JsonSerializable(typeof(UnionField))]
    [JsonSerializable(typeof(ObjectField))]
    [JsonSerializable(typeof(DiffResult))]
    [JsonSerializable(typeof(Expression))]
    public partial class PolyglotJsonContext : JsonSerializerContext
    {
    }
}
