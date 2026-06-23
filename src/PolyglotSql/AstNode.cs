using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PolyglotSql
{
    public class AstNode
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = "";

        [JsonPropertyName("expression")]
        public JsonElement? Expression { get; set; }

        [JsonExtensionData]
        public Dictionary<string, JsonElement> AdditionalProperties { get; set; } = new Dictionary<string, JsonElement>();

        public override string ToString()
        {
            return $"AstNode {{ Type = {Type} }}";
        }
    }

    public class AstParser
    {
        public static List<AstNode> Parse(string json)
        {
            var nodes = new List<AstNode>();
            using var doc = JsonDocument.Parse(json);

            if (doc.RootElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var element in doc.RootElement.EnumerateArray())
                {
                    nodes.Add(ParseNode(element));
                }
            }

            return nodes;
        }

        private static AstNode ParseNode(JsonElement element)
        {
            var node = new AstNode();

            if (element.ValueKind == JsonValueKind.String)
            {
                node.Type = element.GetString() ?? "";
                return node;
            }

            if (element.ValueKind != JsonValueKind.Object)
                return node;

            foreach (var prop in element.EnumerateObject())
            {
                if (prop.NameEquals("type"))
                {
                    node.Type = prop.Value.GetString() ?? "";
                }
                else if (prop.NameEquals("expression"))
                {
                    node.Expression = prop.Value.Clone();
                }
                else
                {
                    node.AdditionalProperties[prop.Name] = prop.Value.Clone();
                }
            }

            return node;
        }
    }
}
