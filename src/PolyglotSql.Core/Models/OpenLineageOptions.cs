using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PolyglotSql.Models
{
    public class DialectJsonConverter : JsonConverter<Dialect>
    {
        public override Dialect Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var s = reader.GetString();
            return Enum.TryParse<Dialect>(s, true, out var v) ? v : Dialect.Generic;
        }

        public override void Write(Utf8JsonWriter writer, Dialect value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString().ToLowerInvariant());
        }
    }

    public record OpenLineageOptions
    {
        [JsonPropertyName("dialect")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonConverter(typeof(DialectJsonConverter))]
        public Dialect? Dialect { get; set; }

        [JsonPropertyName("producer")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Producer { get; set; }

        [JsonPropertyName("datasetNamespace")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string DatasetNamespace { get; set; }

        [JsonPropertyName("datasetMappings")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Dictionary<string, OpenLineageDatasetId> DatasetMappings { get; set; }

        [JsonPropertyName("outputDataset")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public OpenLineageDatasetId OutputDataset { get; set; }

        [JsonPropertyName("schema")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ValidationSchema Schema { get; set; }

        [JsonPropertyName("jobNamespace")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string JobNamespace { get; set; }

        [JsonPropertyName("jobName")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string JobName { get; set; }

        [JsonPropertyName("eventTime")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string EventTime { get; set; }

        [JsonPropertyName("runId")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string RunId { get; set; }

        [JsonPropertyName("eventType")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonConverter(typeof(JsonStringEnumConverter<OpenLineageRunEventType>))]
        public OpenLineageRunEventType? EventType { get; set; }
    }
}
