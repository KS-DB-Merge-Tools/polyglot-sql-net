using System.Text.Json.Serialization;

namespace PolyglotSql.Models
{
    public class TranspileOptions
    {
        [JsonPropertyName("pretty")]
        public bool Pretty { get; set; }

        [JsonPropertyName("unsupportedLevel")]
        public UnsupportedLevel UnsupportedLevel { get; set; } = UnsupportedLevel.warn;

        [JsonPropertyName("maxUnsupported")]
        public int MaxUnsupported { get; set; }
    }
}
