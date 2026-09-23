using System.Text.Json.Serialization;

namespace PolyglotSql.Models
{
    public record BuildRequest
    {
        public const int ProtocolVersion = 1;

        [JsonPropertyName("version")]
        public int Version { get; set; } = ProtocolVersion;

        [JsonPropertyName("read_dialect")]
        [JsonConverter(typeof(DialectJsonConverter))]
        public Dialect ReadDialect { get; set; } = Dialect.Generic;

        [JsonPropertyName("plan")]
        public BuilderPlan Plan { get; set; }

        [JsonPropertyName("output")]
        public BuilderOutput Output { get; set; } = new BuilderOutput.Ast();
    }
}
