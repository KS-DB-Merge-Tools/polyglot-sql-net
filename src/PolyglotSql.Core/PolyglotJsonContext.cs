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
    [JsonSerializable(typeof(ValidationSchema))]
    [JsonSerializable(typeof(SchemaTable))]
    [JsonSerializable(typeof(SchemaColumn))]
    [JsonSerializable(typeof(QualifyTablesOptions))]
    [JsonSerializable(typeof(RenameTablesOptions))]
    [JsonSerializable(typeof(Dictionary<string, string>))]
    [JsonSerializable(typeof(Expression))]
    public partial class PolyglotJsonContext : JsonSerializerContext
    {
    }
}
