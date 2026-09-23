using System.Text.Json.Serialization;

namespace PolyglotSql.Models
{
    // One entry in a query's ordered output description.
    // Mirrors the native Rust `OutputColumn` enum, which serializes as {"kind":"<variant>",...}.
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "kind")]
    [JsonDerivedType(typeof(Named), "named")]
    [JsonDerivedType(typeof(Unnamed), "unnamed")]
    [JsonDerivedType(typeof(Wildcard), "wildcard")]
    public abstract record OutputColumn
    {
        private OutputColumn() { }

        // A single output column with a known name.
        public sealed record Named : OutputColumn
        {
            [JsonPropertyName("name")]
            public string Name { get; set; }

            // Zero-based output ordinal, when knowable.
            [JsonPropertyName("ordinal")]
            public int? Ordinal { get; set; }
        }

        // A single output column whose database-provided name is not reliable.
        public sealed record Unnamed : OutputColumn
        {
            [JsonPropertyName("ordinal")]
            public int? Ordinal { get; set; }
        }

        // An unresolved wildcard that contributes an unknown number of columns.
        public sealed record Wildcard : OutputColumn
        {
            // Optional table or source qualifier from `table.*`.
            [JsonPropertyName("qualifier")]
            public string Qualifier { get; set; }

            // First possible output ordinal, when no earlier wildcard exists.
            [JsonPropertyName("startOrdinal")]
            public int? StartOrdinal { get; set; }
        }
    }
}
