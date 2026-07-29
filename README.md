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

This project/package includes native polyglot binaries for win-x64, win-x86, and linux-x64 target runtimes. Use the following call for  initialization:

```cs
BundleInitializer.Initialize();
```

Please note that packaged native binary works only if your project has <RuntimeIdentifier> matching to one of supported runtimes or if you're using it in the .NET framework project. In other cases please consider using the **PolyglotSql.Core** package which provides more flexibility.

### Usage Example

```cs
string result = Polyglot
	.Transpile(
		"SELECT `id`, `name` FROM `person` LIMIT 10;",
		Dialect.MySQL,
		Dialect.TSQL)
	.FirstOrDefault();

Console.WriteLine(result); // SELECT TOP 10 [id], [name] FROM [person]
```

## Native Polyglot Version

The current version of this library is based on the Polyglot version from file [polyglot-version.txt](polyglot-version.txt). The Bundle package includes binaries built from that version. If you use the Core package with some other version, it may fail to work with some other verions because of missing methods or other signature changes.

## Credits & Acknowledgments

This project is a managed .NET wrapper around the excellent [Polyglot](https://github.com/tobilg/polyglot) Rust library developed by Tobias Müller. 

The core transpilation logic relies on the foundation inspired by the [SQLGlot](https://github.com/tobymao/sqlglot) Python library developed by Toby Mao.

## License

[MIT](LICENSE)  

