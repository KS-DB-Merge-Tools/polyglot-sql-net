using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PolyglotSql.Models
{
    [JsonConverter(typeof(JsonStringEnumConverter<SourceKind>))]
    public enum SourceKind
    {
        unknown,
        root,
        cte,
        derived_table,
        subquery,
        table,
        @virtual,
        view,
        lateral,
        unnest
    }

    public record LineageNode
    {
        public string Name { get; set; } = string.Empty;

        public Expression Expression { get; set; }

        public Expression Source { get; set; }

        public List<LineageNode> Downstream { get; set; } = new List<LineageNode>();

        public string SourceName { get; set; } = string.Empty;

        public SourceKind SourceKind { get; set; } = SourceKind.unknown;

        public string SourceAlias { get; set; } = string.Empty;

        public string ReferenceNodeName { get; set; } = string.Empty;

        public override string ToString()
        {
            return $"LineageNode {{ Name: {Name}, SourceName: {SourceName}, SourceKind: {SourceKind}, Downstream: {Downstream.Count} }}";
        }
    }

    public class LineageParser
    {
        public static LineageNode Parse(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return null;

            var doc = JsonDocument.Parse(json);
            return ParseNode(doc.RootElement);
        }

        private static LineageNode ParseNode(JsonElement elem)
        {
            var node = new LineageNode();

            if (elem.TryGetProperty("name", out var name))
                node.Name = name.GetString() ?? string.Empty;

            if (elem.TryGetProperty("expression", out var expr) && expr.ValueKind != JsonValueKind.Null)
                node.Expression = new Expression(expr.Clone());

            if (elem.TryGetProperty("source", out var src) && src.ValueKind != JsonValueKind.Null)
                node.Source = new Expression(src.Clone());

            if (elem.TryGetProperty("source_name", out var sourceName))
                node.SourceName = sourceName.GetString() ?? string.Empty;

            if (elem.TryGetProperty("source_kind", out var sourceKind))
            {
                var kindStr = sourceKind.GetString() ?? string.Empty;
                node.SourceKind = Enum.TryParse<SourceKind>(kindStr, false, out var kind) ? kind : SourceKind.unknown;
            }

            if (elem.TryGetProperty("source_alias", out var sourceAlias))
                node.SourceAlias = sourceAlias.GetString() ?? string.Empty;

            if (elem.TryGetProperty("reference_node_name", out var refName))
                node.ReferenceNodeName = refName.GetString() ?? string.Empty;

            if (elem.TryGetProperty("downstream", out var downstream) && downstream.ValueKind == JsonValueKind.Array)
            {
                foreach (var child in downstream.EnumerateArray())
                    node.Downstream.Add(ParseNode(child.Clone()));
            }

            return node;
        }
    }
}
