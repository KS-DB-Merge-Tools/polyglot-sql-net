# Polyglot SQL .NET

Polyglot SQL .NET is a managed .NET wrapper around the [Polyglot](https://github.com/tobilg/polyglot) Rust library.

## PolyglotSql.Core package
 
This package is just a .NET layer only, it does not include native polyglot binaries. You'll need to get one (for example from polyglot releases) and make a provider registration call:

```cs
var provider = new DynamicRustProvider(libPath);
Polyglot.RegisterProvider(provider);
```

If you want to make it work out of the box without searching for the native polyglot library, please consider using the **PolyglotSql.Bundle** package - it goes with binaries for win-x64, win-x86, and linux-x64 runtimes.