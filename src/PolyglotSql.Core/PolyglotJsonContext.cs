using System.Text.Json;
using System.Text.Json.Serialization;

namespace PolyglotSql
{
    [JsonSerializable(typeof(string[]))]
    internal partial class PolyglotJsonContext : JsonSerializerContext
    {
    }
}
