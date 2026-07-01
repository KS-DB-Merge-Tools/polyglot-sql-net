using System.Text.Json;
using System.Text.Json.Serialization;

namespace PolyglotSql
{
    [JsonSerializable(typeof(string[]))]
    [JsonSerializable(typeof(DataType))]
    [JsonSerializable(typeof(StructField))]
    [JsonSerializable(typeof(UnionField))]
    [JsonSerializable(typeof(ObjectField))]
    public partial class PolyglotJsonContext : JsonSerializerContext
    {
    }
}
