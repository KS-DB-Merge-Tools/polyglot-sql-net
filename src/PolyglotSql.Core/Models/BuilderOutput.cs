using System.Text.Json.Serialization;

namespace PolyglotSql.Models
{
    // Output mode of polyglot_build. Mirrors the native Rust `BuilderOutput` enum,
    // which serializes as {"kind":"ast"} or {"kind":"sql","dialect":"<name>"}.
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "kind")]
    [JsonDerivedType(typeof(Ast), "ast")]
    [JsonDerivedType(typeof(Sql), "sql")]
    public abstract record BuilderOutput
    {
        private BuilderOutput() { }

        public sealed record Ast : BuilderOutput;

        public sealed record Sql : BuilderOutput
        {
            [JsonPropertyName("dialect")]
            [JsonConverter(typeof(DialectJsonConverter))]
            public Dialect Dialect { get; set; } = Dialect.Generic;
        }
    }
}
