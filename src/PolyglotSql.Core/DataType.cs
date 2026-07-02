using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PolyglotSql
{
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "data_type")]
    [JsonDerivedType(typeof(DataTypeBool), "boolean")]
    [JsonDerivedType(typeof(TinyInt), "tiny_int")]
    [JsonDerivedType(typeof(SmallInt), "small_int")]
    [JsonDerivedType(typeof(Int), "int")]
    [JsonDerivedType(typeof(UInt), "u_int")]
    [JsonDerivedType(typeof(BigInt), "big_int")]
    [JsonDerivedType(typeof(Float), "float")]
    [JsonDerivedType(typeof(Double), "double")]
    [JsonDerivedType(typeof(Decimal), "decimal")]
    [JsonDerivedType(typeof(Char), "char")]
    [JsonDerivedType(typeof(VarChar), "var_char")]
    [JsonDerivedType(typeof(StringType), "string")]
    [JsonDerivedType(typeof(Text), "text")]
    [JsonDerivedType(typeof(TextWithLength), "text_with_length")]
    [JsonDerivedType(typeof(Binary), "binary")]
    [JsonDerivedType(typeof(VarBinary), "var_binary")]
    [JsonDerivedType(typeof(Blob), "blob")]
    [JsonDerivedType(typeof(Bit), "bit")]
    [JsonDerivedType(typeof(VarBit), "var_bit")]
    [JsonDerivedType(typeof(Date), "date")]
    [JsonDerivedType(typeof(Time), "time")]
    [JsonDerivedType(typeof(Timestamp), "timestamp")]
    [JsonDerivedType(typeof(IntervalType), "interval")]
    [JsonDerivedType(typeof(Json), "json")]
    [JsonDerivedType(typeof(JsonB), "json_b")]
    [JsonDerivedType(typeof(Uuid), "uuid")]
    [JsonDerivedType(typeof(ArrayType), "array")]
    [JsonDerivedType(typeof(ListType), "list")]
    [JsonDerivedType(typeof(StructType), "struct")]
    [JsonDerivedType(typeof(MapType), "map")]
    [JsonDerivedType(typeof(EnumType), "enum")]
    [JsonDerivedType(typeof(SetType), "set")]
    [JsonDerivedType(typeof(UnionType), "union")]
    [JsonDerivedType(typeof(VectorType), "vector")]
    [JsonDerivedType(typeof(ObjectType), "object")]
    [JsonDerivedType(typeof(NullableType), "nullable")]
    [JsonDerivedType(typeof(CustomType), "custom")]
    [JsonDerivedType(typeof(GeometryType), "geometry")]
    [JsonDerivedType(typeof(GeographyType), "geography")]
    [JsonDerivedType(typeof(CharacterSetType), "character_set")]
    [JsonDerivedType(typeof(Unknown), "unknown")]
    public abstract partial record DataType
    {
        private DataType() { }

        // Numeric types

        // Not Boolean to avoid conflicts with System.Boolean
        public sealed record DataTypeBool : DataType;

        public sealed record TinyInt : DataType
        {
            [JsonPropertyName("length")]
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public uint? Length { get; set; }
        }

        public sealed record SmallInt : DataType
        {
            [JsonPropertyName("length")]
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public uint? Length { get; set; }
        }

        public sealed record Int : DataType
        {
            [JsonPropertyName("length")]
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public uint? Length { get; set; }
            [JsonPropertyName("integer_spelling")]
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
            public bool IntegerSpelling { get; set; }
        }

        public sealed record UInt : DataType
        {
            [JsonPropertyName("length")]
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public uint? Length { get; set; }
        }

        public sealed record BigInt : DataType
        {
            [JsonPropertyName("length")]
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public uint? Length { get; set; }
        }

        public sealed record Float : DataType
        {
            [JsonPropertyName("precision")]
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public uint? Precision { get; set; }
            [JsonPropertyName("scale")]
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public uint? Scale { get; set; }
            [JsonPropertyName("real_spelling")]
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
            public bool RealSpelling { get; set; }
        }

        public sealed record Double : DataType
        {
            [JsonPropertyName("precision")]
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public uint? Precision { get; set; }
            [JsonPropertyName("scale")]
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public uint? Scale { get; set; }
        }

        public sealed record Decimal : DataType
        {
            [JsonPropertyName("precision")]
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public uint? Precision { get; set; }
            [JsonPropertyName("scale")]
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public uint? Scale { get; set; }
        }

        // String types
        public sealed record Char : DataType
        {
            [JsonPropertyName("length")]
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public uint? Length { get; set; }
        }

        public sealed record VarChar : DataType
        {
            [JsonPropertyName("length")]
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public uint? Length { get; set; }
            [JsonPropertyName("parenthesized_length")]
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
            public bool ParenthesizedLength { get; set; }
        }

        public sealed record StringType : DataType
        {
            [JsonPropertyName("length")]
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public uint? Length { get; set; }
        }

        public sealed record Text : DataType;

        public sealed record TextWithLength : DataType
        {
            [JsonPropertyName("length")]
            public uint Length { get; set; }
        }

        // Binary types
        public sealed record Binary : DataType
        {
            [JsonPropertyName("length")]
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public uint? Length { get; set; }
        }

        public sealed record VarBinary : DataType
        {
            [JsonPropertyName("length")]
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public uint? Length { get; set; }
        }

        public sealed record Blob : DataType;

        // Bit types
        public sealed record Bit : DataType
        {
            [JsonPropertyName("length")]
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public uint? Length { get; set; }
        }

        public sealed record VarBit : DataType
        {
            [JsonPropertyName("length")]
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public uint? Length { get; set; }
        }

        // Date/Time types
        public sealed record Date : DataType;

        public sealed record Time : DataType
        {
            [JsonPropertyName("precision")]
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public uint? Precision { get; set; }
            [JsonPropertyName("timezone")]
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
            public bool Timezone { get; set; }
        }

        public sealed record Timestamp : DataType
        {
            [JsonPropertyName("precision")]
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public uint? Precision { get; set; }
            [JsonPropertyName("timezone")]
            public bool Timezone { get; set; }
        }

        public sealed record IntervalType : DataType
        {
            [JsonPropertyName("unit")]
            public string Unit { get; set; }
            [JsonPropertyName("to")]
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public string To { get; set; }
        }

        // JSON types
        public sealed record Json : DataType;

        public sealed record JsonB : DataType;

        // UUID type
        public sealed record Uuid : DataType;

        // Array/List types
        public sealed record ArrayType : DataType
        {
            [JsonPropertyName("element_type")]
            public DataType ElementType { get; set; }
            [JsonPropertyName("dimension")]
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public uint? Dimension { get; set; }
        }

        public sealed record ListType : DataType
        {
            [JsonPropertyName("element_type")]
            public DataType ElementType { get; set; }
        }

        // Struct/Map types
        public sealed record StructType : DataType
        {
            [JsonPropertyName("fields")]
            public List<StructField> Fields { get; set; }
            [JsonPropertyName("nested")]
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
            public bool Nested { get; set; }
        }

        public sealed record MapType : DataType
        {
            [JsonPropertyName("key_type")]
            public DataType KeyType { get; set; }
            [JsonPropertyName("value_type")]
            public DataType ValueType { get; set; }
        }

        // Enum/Set types
        public sealed record EnumType : DataType
        {
            [JsonPropertyName("values")]
            public List<string> Values { get; set; }
            [JsonPropertyName("assignments")]
            public List<string> Assignments { get; set; }
        }

        public sealed record SetType : DataType
        {
            [JsonPropertyName("values")]
            public List<string> Values { get; set; }
        }

        // Union type - uses tuple (name, DataType) format in Rust
        public sealed record UnionType : DataType
        {
            [JsonPropertyName("fields")]
            [JsonConverter(typeof(UnionFieldConverter))]
            public List<UnionField> Fields { get; set; }
        }

        // Vector type
        public sealed record VectorType : DataType
        {
            [JsonPropertyName("element_type")]
            public DataType ElementType { get; set; }
            [JsonPropertyName("dimension")]
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public uint? Dimension { get; set; }
        }

        // Object type - uses tuple (name, DataType, not_null) format in Rust
        public sealed record ObjectType : DataType
        {
            [JsonPropertyName("fields")]
            [JsonConverter(typeof(ObjectFieldConverter))]
            public List<ObjectField> Fields { get; set; }
            [JsonPropertyName("modifier")]
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public string Modifier { get; set; }
        }

        // Nullable wrapper
        public sealed record NullableType : DataType
        {
            [JsonPropertyName("inner")]
            public DataType Inner { get; set; }
        }

        // Custom/User-defined
        public sealed record CustomType : DataType
        {
            [JsonPropertyName("name")]
            public string Name { get; set; }
        }

        // Spatial types
        public sealed record GeometryType : DataType
        {
            [JsonPropertyName("subtype")]
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public string Subtype { get; set; }
            [JsonPropertyName("srid")]
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public uint? Srid { get; set; }
        }

        public sealed record GeographyType : DataType
        {
            [JsonPropertyName("subtype")]
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public string Subtype { get; set; }
            [JsonPropertyName("srid")]
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public uint? Srid { get; set; }
        }

        // Character Set
        public sealed record CharacterSetType : DataType
        {
            [JsonPropertyName("name")]
            public string Name { get; set; }
        }

        // Unknown
        public sealed record Unknown : DataType;
    }

    public class StructField
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("data_type")]
        public DataType Type { get; set; }
    }

    public class UnionField
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("data_type")]
        public DataType Type { get; set; }
    }

    public class ObjectField
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("data_type")]
        public DataType Type { get; set; }
        [JsonPropertyName("not_null")]
        public bool NotNull { get; set; }
    }

    public class UnionFieldConverter : JsonConverter<List<UnionField>>
    {
        public override List<UnionField> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartArray)
                throw new JsonException();

            var fields = new List<UnionField>();
            while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
            {
                if (reader.TokenType == JsonTokenType.StartArray)
                {
                    var field = ReadTuple(ref reader, options);
                    fields.Add(field);
                }
            }
            return fields;
        }

        private UnionField ReadTuple(ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            string name = null;
            DataType dataType = null;
            int index = 0;
            while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
            {
                if (reader.TokenType == JsonTokenType.String)
                {
                    if (index == 0)
                        name = reader.GetString();
                }
                else if (reader.TokenType == JsonTokenType.StartObject)
                {
                    dataType = (DataType)JsonSerializer.Deserialize(ref reader, typeof(DataType), options)!;
                }
                index++;
            }
            return new UnionField { Name = name, Type = dataType };
        }

        public override void Write(Utf8JsonWriter writer, List<UnionField> value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            foreach (var field in value)
            {
                writer.WriteStartArray();
                writer.WriteStringValue(field.Name);
                JsonSerializer.Serialize(writer, field.Type, options);
                writer.WriteEndArray();
            }
            writer.WriteEndArray();
        }
    }

    public class ObjectFieldConverter : JsonConverter<List<ObjectField>>
    {
        public override List<ObjectField> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartArray)
                throw new JsonException();

            var fields = new List<ObjectField>();
            while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
            {
                if (reader.TokenType == JsonTokenType.StartArray)
                {
                    var field = ReadTuple(ref reader, options);
                    fields.Add(field);
                }
            }
            return fields;
        }

        private ObjectField ReadTuple(ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            string name = null;
            DataType dataType = null;
            bool notNull = false;
            int index = 0;
            while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
            {
                if (reader.TokenType == JsonTokenType.String)
                {
                    if (index == 0)
                        name = reader.GetString();
                }
                else if (reader.TokenType == JsonTokenType.True)
                {
                    if (index == 2)
                        notNull = true;
                }
                else if (reader.TokenType == JsonTokenType.False)
                {
                    if (index == 2)
                        notNull = false;
                }
                else if (reader.TokenType == JsonTokenType.StartObject)
                {
                    dataType = (DataType)JsonSerializer.Deserialize(ref reader, typeof(DataType), options)!;
                }
                index++;
            }
            return new ObjectField { Name = name, Type = dataType, NotNull = notNull };
        }

        public override void Write(Utf8JsonWriter writer, List<ObjectField> value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            foreach (var field in value)
            {
                writer.WriteStartArray();
                writer.WriteStringValue(field.Name);
                JsonSerializer.Serialize(writer, field.Type, options);
                writer.WriteBooleanValue(field.NotNull);
                writer.WriteEndArray();
            }
            writer.WriteEndArray();
        }
    }
}