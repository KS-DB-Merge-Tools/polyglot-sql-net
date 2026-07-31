# Polyglot SQL .NET

A .NET FFI wrapper for the Polyglot, a Rust SQL transpiler. Enables high-performance parsing, formatting, AST generation, and translation between more than 30 SQL dialects.

## How to Use

There are two options: **PolyglotSql.Core** to use with your own Polyglot native library, or **PolyglotSql.Bundle** to use it out-of-the-box. Packages with the same names are available on the [nuget](https://www.nuget.org/profiles/ksdbmergetools).

### PolyglotSql.Core

Take native Polyglot library (for example from Polyglot reseases) and use the following code for initialization:

```cs
var provider = new DynamicRustProvider(libPath);
Polyglot.RegisterProvider(provider);
```

### PolyglotSql.Bundle

This project/package includes native polyglot binaries for `win-x64`, `win-x86`, and `linux-x64` target runtimes. Use the following call for initialization:

```cs
BundleInitializer.Initialize();
```

Please note that packaged native binary works only if your project has `<RuntimeIdentifier>` matching to one of supported runtimes or if you're using it in the .NET framework project. In other cases please consider using the **PolyglotSql.Core** package which provides more flexibility.

### Usage Example

Simple transpile example:

```cs
string result = Polyglot
	.Transpile(
		"SELECT `id`, `name` FROM `person` LIMIT 10;",
		Dialect.MySQL,
		Dialect.TSQL)
	.FirstOrDefault();

Console.WriteLine(result); // SELECT TOP 10 [id], [name] FROM [person]
```

Almost all objects that Polyglot accepts and returns as JSON are represented as separate classes in the `Polyglot.Models` namespace, with a set of properties from the original library. Here's an example of using the `TranspileOptions` class:

```cs
var options = new TranspileOptions { Pretty = pretty };
string result = Polyglot
	.TranspileWithOptions("SELECT `a` FROM `t`", Dialect.MySQL, Dialect.TSQL, options)
	.FirstOrDefault();

Console.WriteLine(result);
/*
SELECT
  [a]
FROM [t]
*/
```

The only exception is the `Expression` class. At the time of Polyglot SQL .NET creation, the Rust library contained over 700 different AST node types. Creating a similar model on the C# side would require a huge amount of effort, significantly increased the library's size, and, most importantly, it would add an indefinite amount of work in the future to maintain synchronization between this codebase and Rust. Therefore, the decision was made to leave `Expression` as a thin wrapper over JSON:

```cs
[JsonConverter(typeof(ExpressionJsonConverter))]
public record Expression
{
	public JsonElement Json { get; }
	public string ToJsonString();
	public void TransformAll(Action<JsonNode> transform);
}
```

To modify such an Expression, it must be deserialized, edited, and reserialized. To simplify this task for simple `Expression` modifications, the `TransformAll()` method was added, which traverses all JSON nodes and makes the necessary edits. Here's an example of how to force quoted identifiers using this method:

```cs
string sql = "SELECT [a], b FROM t";
var dialect = Models.Dialect.TSQL;

Expression expression = Polyglot.ParseOne(sql, dialect);

expression.TransformAll(node => {
	if (node is JsonObject obj && obj.ContainsKey("quoted"))
	{
		obj["quoted"] = true;
	}
});

var generated = Polyglot.GenerateOne(expression, dialect); // SELECT [a], [b] FROM [t]
```

If `TransformAll()` is not enough for your needs and you still need to deserialize-modify-serialize, and if you need to keep this AOT-compatible, use source generators based on `PolyglotJsonContext` json serialization context:

```cs
var parsedExpression = Polyglot.ParseOne(sql, dialect);
JsonNode rootNode = JsonNode.Parse(parsedExpression.ToJsonString());
// modify rootNode or its children
string modifiedJson = rootNode.ToJsonString();
var modifiedExpression = JsonSerializer.Deserialize(modifiedJson, PolyglotJsonContext.Default.Expression);
string[] result = Polyglot.Generate(new[] { normalizedExpression }, dialect);
```

## Native Polyglot Version

The current version of this library is based on the Polyglot version from file [polyglot-version.txt](polyglot-version.txt). The Bundle package includes binaries built from that version. If you use the Core package with some other version, it may fail to work with some other verions because of missing methods or other signature changes.

## Credits & Acknowledgments

This project is a managed .NET wrapper around the excellent [Polyglot](https://github.com/tobilg/polyglot) Rust library developed by Tobias Müller. 

The core transpilation logic relies on the foundation inspired by the [SQLGlot](https://github.com/tobymao/sqlglot) Python library developed by Toby Mao.

## License

[MIT](LICENSE)  