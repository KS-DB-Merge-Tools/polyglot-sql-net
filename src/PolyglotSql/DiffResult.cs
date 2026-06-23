using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PolyglotSql
{
    public enum DiffEditType
    {
        Insert,
        Remove,
        Move,
        Update,
        Keep
    }

    public class DiffEdit
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = "";

        [JsonPropertyName("expression")]
        public JsonElement? Expression { get; set; }

        [JsonPropertyName("source")]
        public JsonElement? Source { get; set; }

        [JsonPropertyName("target")]
        public JsonElement? Target { get; set; }

        public DiffEditType EditType
        {
            get
            {
                return Type switch
                {
                    "insert" => DiffEditType.Insert,
                    "remove" => DiffEditType.Remove,
                    "move" => DiffEditType.Move,
                    "update" => DiffEditType.Update,
                    "keep" => DiffEditType.Keep,
                    _ => DiffEditType.Keep
                };
            }
        }

        public override string ToString()
        {
            return Type switch
            {
                "insert" => $"Insert: {GetExpressionPreview()}",
                "remove" => $"Remove: {GetExpressionPreview()}",
                "move" => $"Move: {GetSourcePreview()} -> {GetTargetPreview()}",
                "update" => $"Update: {GetSourcePreview()} -> {GetTargetPreview()}",
                "keep" => $"Keep: {GetSourcePreview()} == {GetTargetPreview()}",
                _ => $"Unknown: {Type}"
            };
        }

        private string GetExpressionPreview()
        {
            if (Expression.HasValue)
                return GetJsonPreview(Expression.Value);
            return "(no expression)";
        }

        private string GetSourcePreview()
        {
            if (Source.HasValue)
                return GetJsonPreview(Source.Value);
            return "(no source)";
        }

        private string GetTargetPreview()
        {
            if (Target.HasValue)
                return GetJsonPreview(Target.Value);
            return "(no target)";
        }

        private static string GetJsonPreview(JsonElement element)
        {
            string raw = element.GetRawText();
            if (raw.Length > 60)
                return raw.Substring(0, 60) + "...";
            return raw;
        }
    }

    public class DiffResult
    {
        public List<DiffEdit> Edits { get; set; } = new List<DiffEdit>();

        public int InsertCount => Edits.FindAll(e => e.EditType == DiffEditType.Insert).Count;
        public int RemoveCount => Edits.FindAll(e => e.EditType == DiffEditType.Remove).Count;
        public int MoveCount => Edits.FindAll(e => e.EditType == DiffEditType.Move).Count;
        public int UpdateCount => Edits.FindAll(e => e.EditType == DiffEditType.Update).Count;
        public int KeepCount => Edits.FindAll(e => e.EditType == DiffEditType.Keep).Count;

        public bool AreEqual => InsertCount == 0 && RemoveCount == 0 && MoveCount == 0 && UpdateCount == 0;

        public override string ToString()
        {
            return $"DiffResult {{ Total: {Edits.Count}, Inserts: {InsertCount}, Removes: {RemoveCount}, Moves: {MoveCount}, Updates: {UpdateCount}, Keeps: {KeepCount} }}";
        }
    }

    public class DiffParser
    {
        public static DiffResult Parse(string json)
        {
            var result = new DiffResult();
            using var doc = JsonDocument.Parse(json);

            if (doc.RootElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var element in doc.RootElement.EnumerateArray())
                {
                    result.Edits.Add(ParseEdit(element));
                }
            }

            return result;
        }

        private static DiffEdit ParseEdit(JsonElement element)
        {
            var edit = new DiffEdit
            {
                Type = element.GetProperty("type").GetString() ?? ""
            };

            if (element.TryGetProperty("expression", out var expr))
                edit.Expression = expr.Clone();

            if (element.TryGetProperty("source", out var source))
                edit.Source = source.Clone();

            if (element.TryGetProperty("target", out var target))
                edit.Target = target.Clone();

            return edit;
        }
    }
}
