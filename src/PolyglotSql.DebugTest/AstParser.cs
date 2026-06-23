using System;
using System.Collections.Generic;
using System.Text.Json;

namespace SqlGlotDotNet.DebugTest;

public class AstNode
{
    public string Type { get; set; } = "";
    public Dictionary<string, object?> Properties { get; set; } = new();
    public List<AstNode> Children { get; set; } = new();

    public override string ToString()
    {
        return $"AstNode({Type})";
    }
}

public static class AstParser
{
    public static List<AstNode> Parse(string json)
    {
        var nodes = new List<AstNode>();
        using var doc = JsonDocument.Parse(json);
        foreach (var element in doc.RootElement.EnumerateArray())
        {
            nodes.Add(ParseNode(element));
        }
        return nodes;
    }

    private static AstNode ParseNode(JsonElement element)
    {
        var node = new AstNode();

        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in element.EnumerateObject())
            {
                string key = property.Name;
                if (property.Value.ValueKind == JsonValueKind.Object)
                {
                    node.Type = key;
                    var child = ParseObjectNode(property.Value, key);
                    node.Children.Add(child);
                }
                else if (property.Value.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in property.Value.EnumerateArray())
                    {
                        if (item.ValueKind == JsonValueKind.Object)
                        {
                            node.Children.Add(ParseNode(item));
                        }
                    }
                }
                else
                {
                    node.Properties[key] = GetValue(property.Value);
                }
            }
        }

        if (string.IsNullOrEmpty(node.Type) && node.Children.Count > 0)
        {
            node.Type = node.Children[0].Type;
        }

        return node;
    }

    private static AstNode ParseObjectNode(JsonElement element, string parentKey)
    {
        var node = new AstNode { Type = parentKey };

        foreach (var property in element.EnumerateObject())
        {
            string key = property.Name;

            if (property.Value.ValueKind == JsonValueKind.Object)
            {
                node.Children.Add(ParseObjectNode(property.Value, key));
            }
            else if (property.Value.ValueKind == JsonValueKind.Array)
            {
                var listNode = new AstNode { Type = key };
                foreach (var item in property.Value.EnumerateArray())
                {
                    if (item.ValueKind == JsonValueKind.Object)
                    {
                        listNode.Children.Add(ParseNode(item));
                    }
                    else
                    {
                        listNode.Properties["_" + listNode.Properties.Count] = GetValue(item);
                    }
                }
                node.Children.Add(listNode);
            }
            else if (property.Value.ValueKind == JsonValueKind.Null)
            {
                node.Properties[key] = null;
            }
            else
            {
                node.Properties[key] = GetValue(property.Value);
            }
        }

        return node;
    }

    private static object? GetValue(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString() ?? "",
            JsonValueKind.Number => element.TryGetInt64(out long l) ? l : element.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null,
            _ => element.ToString()
        };
    }
}
