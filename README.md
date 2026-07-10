# Polyglot SQL .NET

A .NET FFI wrapper for the Polyglot, a Rust SQL transpiler. Enables high-performance parsing, formatting, AST generation, and translation between more than 30 SQL dialects.

## Projects

This repository contains a .NET solution with the following projects:

- **PolyglotSql.Core** (`netstandard2.0`) — the core wrapper library that calls into the polyglot Rust engine via C FFI. AOT-compatible.
- **PolyglotSql.Bundle** (`netstandard2.0`) — PolyglotSql.Core plus the prebuilt Rust native libraries, so the package works out of the box on Windows and Linux.
- **PolyglotSql.Tests** (`.NET 8` + xUnit) — unit tests for the wrapper.

## Requirements

- .NET 8 SDK (for building and testing)
- A prebuilt `libpolyglot_sql_ffi` native library (bundled automatically via PolyglotSql.Bundle, or supplied by your own Rust build)

## Credits & Acknowledgments

This project is a managed .NET wrapper around the excellent [polyglot](https://github.com/tobilg/polyglot) Rust library developed by Tobias Müller. 

The core transpilation logic relies on the foundation inspired by the [SQLGlot](https://github.com/tobymao/sqlglot) Python library developed by Toby Mao.

## Licenses

[MIT](LICENSE)  
[polyglot MIT](licenses/POLYGLOT_LICENSE)
[sqlglot MIT](licenses/SQLGLOT_LICENSE)

