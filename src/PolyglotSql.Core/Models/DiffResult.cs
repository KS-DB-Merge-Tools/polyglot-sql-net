using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PolyglotSql.Models
{
    public enum DiffEditType
    {
        Unknown,
        Insert,
        Remove,
        Move,
        Update,
        Keep
    }

    public class DiffEdit
    {
        [JsonPropertyName("type")]
        public DiffEditType Type { get; set; } = DiffEditType.Unknown;

        [JsonPropertyName("expression")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Expression Expression { get; set; }

        [JsonPropertyName("source")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Expression Source { get; set; }

        [JsonPropertyName("target")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Expression Target { get; set; }
    }

    public class DiffResult
    {
        public List<DiffEdit> Edits { get; set; } = new List<DiffEdit>();

        public int InsertCount { get; set; }
        public int RemoveCount { get; set; }
        public int MoveCount { get; set; }
        public int UpdateCount { get; set; }
        public int KeepCount { get; set; }

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
            var doc = JsonDocument.Parse(json);
            var edits = new List<DiffEdit>();

            foreach (var elem in doc.RootElement.EnumerateArray())
            {
                var typeString = elem.GetProperty("type").GetString() ?? "";
                var edit = new DiffEdit
                {
                    Type = Enum.TryParse<DiffEditType>(typeString, true, out var parsedType) ? parsedType : DiffEditType.Unknown
                };

                if (elem.TryGetProperty("expression", out var expr))
                {
                    edit.Expression = new Expression(expr);
                }

                if (elem.TryGetProperty("source", out var src))
                {
                    edit.Source = new Expression(src);
                }

                if (elem.TryGetProperty("target", out var tgt))
                {
                    edit.Target = new Expression(tgt);
                }

                edits.Add(edit);
            }

            result.Edits = edits;

            // Count edits by type
            int insertCount = 0, removeCount = 0, moveCount = 0, updateCount = 0, keepCount = 0;
            foreach (var edit in result.Edits)
            {
                switch (edit.Type)
                {
                    case DiffEditType.Insert: insertCount++; break;
                    case DiffEditType.Remove: removeCount++; break;
                    case DiffEditType.Move: moveCount++; break;
                    case DiffEditType.Update: updateCount++; break;
                    case DiffEditType.Keep: keepCount++; break;
                }
            }
            result.InsertCount = insertCount;
            result.RemoveCount = removeCount;
            result.MoveCount = moveCount;
            result.UpdateCount = updateCount;
            result.KeepCount = keepCount;

            return result;
        }
    }
}
