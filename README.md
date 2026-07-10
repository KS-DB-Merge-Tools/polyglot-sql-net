# Polyglot SQL .NET

A .NET FFI wrapper for the Polyglot, a Rust SQL transpiler. Enables high-performance parsing, formatting, AST generation, and translation between more than 30 SQL dialects.

## How to Use

There are two options: **PolyglotSql.Core** to use with your own Polyglot native library, or **PolyglotSql.Bundle** to use it out-of-the-box. Packages with the same names are available on the [nuget](https://www.nuget.org/profiles/ksdbmergetools).

### PolyglotSql.Core

Take native Polyglot library (for example from Polyglot reseases) and use the following code for initialization:

```
var provider = new DynamicRustProvider(libPath);
Polyglot.RegisterProvider(provider);
```

### PolyglotSql.Bundle

This project/package includes native polyglot binaries for win-x64, win-x86, and linux-x64 target runtimes. Use the following call for  initialization:

```
BundleInitializer.Initialize();
```

Please note that packaged native binary works only if your project has <RuntimeIdentifier> matching to one of supported runtimes or if you're using it in the .NET framework project. In other cases please consider using the **PolyglotSql.Core** package which provides more flexibility.

### Usage Example

```
string result = Polyglot
	.Transpile(
		"SELECT `id`, `name` FROM `person` LIMIT 10;",
		Dialect.MySQL,
		Dialect.TSQL)
	.FirstOrDefault();

Console.WriteLine(result); // SELECT TOP 10 [id], [name] FROM [person]
```

## Native Polyglot Version

The initial release of this library is based on Polyglot v 0.5.1 - its interface was used to specify list of methods for the Core to build native binaries for the Bundle. So it may fail to work with some earlier verions because of missing methods or with some later verisions because of other signature changes.

## Credits & Acknowledgments

This project is a managed .NET wrapper around the excellent [Polyglot](https://github.com/tobilg/polyglot) Rust library developed by Tobias Müller. 

The core transpilation logic relies on the foundation inspired by the [SQLGlot](https://github.com/tobymao/sqlglot) Python library developed by Toby Mao.

## License

[MIT](LICENSE)  

