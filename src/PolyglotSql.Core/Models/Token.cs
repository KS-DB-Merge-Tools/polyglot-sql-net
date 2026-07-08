using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PolyglotSql.Models
{
    public class TokenTypeJsonConverter : JsonConverter<TokenType>
    {
        public override TokenType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                var str = reader.GetString();
                if (Enum.TryParse<TokenType>(str, ignoreCase: true, out var result))
                    return result;
            }
            else if (reader.TokenType == JsonTokenType.Number && reader.TryGetInt32(out var num))
            {
                return (TokenType)num;
            }
            return TokenType.UNKNOWN;
        }

        public override void Write(Utf8JsonWriter writer, TokenType value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }

    public record Token
    {
        [JsonPropertyName("token_type")]
        [JsonConverter(typeof(TokenTypeJsonConverter))]
        public TokenType TokenType { get; set; }

        [JsonPropertyName("text")]
        public string Text { get; set; }

        [JsonPropertyName("span")]
        public Span Span { get; set; }

        [JsonPropertyName("comments")]
        public string[] Comments { get; set; }

        [JsonPropertyName("trailing_comments")]
        public string[] TrailingComments { get; set; }
    }

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