using System;
using System.Collections.Generic;
using System.Text.Json;

namespace SqlGlotDotNet.DebugTest;

public class DiffEdit
{
    public string Type { get; set; } = "";
    public AstNode Expression { get; set; } = new();

    public override string ToString()
    {
        return $"DiffEdit({Type}, {Expression})";
    }
}

public class DiffResult
{
    public bool AreEqual => Edits.Count == 0;
    public List<DiffEdit> Edits { get; set; } = new();
    public int InsertCount { get; set; }
    public int RemoveCount { get; set; }
    public int UpdateCount { get; set; }

    public override string ToString()
    {
        return $"DiffResult(Equal={AreEqual}, Edits={Edits.Count}, Inserts={InsertCount}, Removes={RemoveCount}, Updates={UpdateCount})";
    }
}

public static class DiffParser
{
    public static DiffResult Parse(string json)
    {
        var result = new DiffResult();

        using var doc = JsonDocument.Parse(json);
        foreach (var element in doc.RootElement.EnumerateArray())
        {
            var edit = new DiffEdit();

            if (element.TryGetProperty("type", out var typeProp))
            {
                edit.Type = typeProp.GetString() ?? "";
            }

            if (element.TryGetProperty("expression", out var exprProp))
            {
                edit.Expression = ParseAstNode(exprProp);
            }

            result.Edits.Add(edit);

            switch (edit.Type)
            {
                case "insert": result.InsertCount++; break;
                case "remove": result.RemoveCount++; break;
                case "update": result.UpdateCount++; break;
            }
        }

        return result;
    }

    private static AstNode ParseAstNode(JsonElement element)
    {
        var node = new AstNode();

        if (element.ValueKind != JsonValueKind.Object)
            return node;

        foreach (var property in element.EnumerateObject())
        {
            string key = property.Name;

            if (property.Value.ValueKind == JsonValueKind.Object)
            {
                node.Type = key;
                node.Children.Add(ParseChildNode(property.Value, key));
            }
            else if (property.Value.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in property.Value.EnumerateArray())
                {
                    if (item.ValueKind == JsonValueKind.Object)
                    {
                        node.Children.Add(ParseAstNode(item));
                    }
                }
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

    private static AstNode ParseChildNode(JsonElement element, string parentKey)
    {
        var node = new AstNode { Type = parentKey };

        foreach (var property in element.EnumerateObject())
        {
            string key = property.Name;

            if (property.Value.ValueKind == JsonValueKind.Object)
            {
                node.Children.Add(ParseChildNode(property.Value, key));
            }
            else if (property.Value.ValueKind == JsonValueKind.Array)
            {
                var listNode = new AstNode { Type = key };
                foreach (var item in property.Value.EnumerateArray())
                {
                    if (item.ValueKind == JsonValueKind.Object)
                    {
                        listNode.Children.Add(ParseAstNode(item));
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
