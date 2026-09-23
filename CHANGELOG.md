
# Change Log

All notable changes to this project will be documented in this file.
 

## [Unreleased] - yyyy-mm-dd
 
### Added

- New methods from Polyglot v0.12.1: `ParseWithOptions`, `ParseOneWithOptions`, `ParseDataTypeWithOptions`, `ValidateWithOptions`, `ValidateWithSchema`, `LineageAt`, `LineageAtWithSchema`, `OutputColumns`, `OutputColumnsWithSchema`, `SetLimit`, `SetOffset`, `SetOrderBy`, `BuildAst`, `BuildSql`
- New models: `ComplexityGuardOptions`, `ParseOptions`, `ValidationOptions`, `SchemaValidationOptions`, `SchemaForeignKey`, `SchemaTableReference`, `SchemaColumnReference`, `QueryOutput`, `OutputColumn`, `BuildRequest`, `BuilderOutput`, `BuilderPlan` (thin JSON wrapper, like `Expression`)
- `ComplexityGuard` property in `TranspileOptions` and `AnalyzeQueryOptions`
- `SchemaTable.ForeignKeys`, `SchemaColumn.References`
- `StructField.Options` and `StructField.Comment`
- Data types `Int128Type`, `UInt8Type`, `UInt16Type`, `UInt32Type`, `UInt64Type`, `UInt128Type`
- `LineageNode.SetBranch` (`SetBranch`, `SetOperator`) - set-operation branch metadata
- `QueryAnalysis.CteFacts`, `BaseTables`, `StarProjections`, `ColumnUses` (`CteFact`, `StarProjectionFact`, `ColumnUseFact`, `ColumnUseReferenceFact`, `ColumnUseContext`, `QuerySourceSpan`)
- `ProjectionFact.Nullability` (`ProjectionNullability`) and `ProjectionFact.TransformFunction` (`TransformFunctionFact`)
- `RelationFact.Catalog`, `Schema`, `Table`
- `SetOperationBranchFact.Role` (`SetOperationBranchRole`)
- API drift scripts

### Changed

- Update Polyglot to v0.12.1
- Breaking change: removed `DataType.UInt` (`u_int`) - has no native counterpart, use the new unsigned integer types
- Breaking change: removed `SourceKind` members `subquery`, `view`, `lateral`, `unnest` - have no native counterpart
- Breaking change: `TokenType` regenerated from the native enum - member names now match the native wire names (e.g. `DCOLON` -> `D_COLON`, `ILIKE` -> `I_LIKE`, `VARCHAR` -> `VAR_CHAR`, 94 renames in total), 108 members added (e.g. `AS`, `BY`, `CAST`, `EOF`), 16 members without a native counterpart removed (e.g. `STREAM`, `ROLE`, `SENTINEL`). Numeric values of existing members are unchanged
 
### Fixed

- `Tokenize` returned `TokenType.UNKNOWN` for multi-word token types (e.g. `D_COLON`) and for token types missing in the .NET enum (e.g. `AS`)
- `TokenType.GROUPING_SETS` had the same numeric value as `TokenType.GROUP_BY`
- `GenerateDataType` failed with "missing field `nested`" for `STRUCT<...>` types (`DataType.StructType.Nested` was omitted from JSON when `false`)


## [0.3.0] - 2026-08-18
 
### Added

- Oracle-specific data types which were added in Polyglot v0.6.3

### Changed
 
- Update Polyglot to v0.9.1

### Fixed

- Minor descriptions/readmes updates

## [0.2.1] - 2026-07-31
 
### Added

### Changed
 
### Fixed

- AOT json serialization issues
- More compact nuget pachage descriptions

## [0.2.0] - 2026-07-29
 
### Added

- Expression.TransformAll(Action<JsonNode> transform) to simplify expression update
- Polyglot.GenerateOne() - similar to Expression.ParseOne()
- Github actions CI with nuget generation, native Polyglot version taken from polyglot-version.txt
- This CHANGEDLOG.md

### Changed
 
### Fixed

- Use Linux system calls from libdl.so.2 with fallback to libdl (previously it was libdl only)

 
## [0.1.0] - 2026-07-10
 
### Added

- Initial version based on Polyglot v0.5.1 
   
### Changed
 
### Fixed