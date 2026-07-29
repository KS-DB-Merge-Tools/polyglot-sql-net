# Polyglot SQL .NET

Polyglot SQL .NET is a managed .NET wrapper around the [Polyglot](https://github.com/tobilg/polyglot) Rust library.

## PolyglotSql.Bundle package
 
This package includes native polyglot binaries for win-x64, win-x86, and linux-x64 target runtimes. To make it work you need only one initialization call:

```cs
BundleInitializer.Initialize();
```

Please note that packaged native binary works only if your project has <RuntimeIdentifier> matching to one of supported runtimes or if you're using it in the .NET framework project. In other cases please consider using the **PolyglotSql.Core** package which provides more flexibility.