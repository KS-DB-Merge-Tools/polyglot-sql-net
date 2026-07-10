using System.Text.Json.Serialization;

namespace PolyglotSql.Models
{
    public record Span
    {
        [JsonPropertyName("start")]
        public int Start { get; set; }

        [JsonPropertyName("end")]
        public int End { get; set; }

        [JsonPropertyName("line")]
        public int Line { get; set; }

        [JsonPropertyName("column")]
        public int Column { get; set; }

        public Span(int start, int end, int line, int column)
        {
            Start = start;
            End = end;
            Line = line;
            Column = column;
        }
    }
}
