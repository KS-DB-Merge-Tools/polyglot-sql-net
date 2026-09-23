using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using PolyglotSql.Models;

namespace PolyglotSql
{
    public static class Polyglot
    {
        internal static INativePolyglot _provider; // internal to test dialect list
        private static readonly object _lock = new object();

        public static void RegisterProvider(INativePolyglot provider)
        {
            if (_provider != null)
                return;

            lock (_lock)
            {
                if (_provider == null)
                    _provider = provider ?? throw new ArgumentNullException(nameof(provider));
            }
        }

        internal static INativePolyglot Provider
        {
            get
            {
                if (_provider == null)
                {
                    throw new InvalidOperationException(
                        "Provider not registered. Call PolyglotSql.RegisterProvider() or use bundle and call BundleInitializer.Initialize().");
                }

                return _provider;
            }
        }

        public static Token[] Tokenize(string sql, Dialect dialect = Dialect.Generic)
            => Provider.Tokenize(sql, dialect);

        public static string[] Transpile(string sql, Dialect fromDialect, Dialect toDialect)
            => Provider.Transpile(sql, fromDialect, toDialect);

        public static string[] TranspileWithOptions(string sql, Dialect fromDialect, Dialect toDialect, TranspileOptions options)
            => Provider.TranspileWithOptions(sql, fromDialect, toDialect, options);

        public static string[] Format(string sql, Dialect dialect)
            => Provider.Format(sql, dialect);

        public static string[] FormatWithOptions(string sql, Dialect dialect, FormatGuardOptions options)
            => Provider.FormatWithOptions(sql, dialect, options);

        public static string[] Optimize(string sql, Dialect dialect)
            => Provider.Optimize(sql, dialect);

        public static Expression[] Parse(string sql, Dialect dialect = Dialect.Generic)
            => Provider.Parse(sql, dialect);

        public static Expression ParseOne(string sql, Dialect dialect = Dialect.Generic)
            => Provider.ParseOne(sql, dialect);

        public static DiffResult Diff(string sql1, string sql2, Dialect dialect = Dialect.Generic)
            => Provider.Diff(sql1, sql2, dialect);

        public static DataType ParseDataType(string sql, Dialect dialect = Dialect.Generic)
            => Provider.ParseDataType(sql, dialect);

        public static string GenerateDataType(DataType dataType, Dialect dialect = Dialect.Generic)
            => Provider.GenerateDataType(dataType, dialect);

        public static LineageNode Lineage(string columnName, string sql, Dialect dialect = Dialect.Generic)
            => Provider.Lineage(columnName, sql, dialect);

        public static LineageNode LineageWithSchema(string columnName, string sql, ValidationSchema schema, Dialect dialect = Dialect.Generic)
            => Provider.LineageWithSchema(columnName, sql, schema, dialect);

        public static string[] SourceTables(string columnName, string sql, Dialect dialect = Dialect.Generic)
            => Provider.SourceTables(columnName, sql, dialect);

        public static Expression[] QualifyTables(Expression[] ast, QualifyTablesOptions options = null)
            => Provider.QualifyTables(ast, options);

        public static Expression[] RenameTablesWithOptions(Expression[] ast, Dictionary<string, string> mapping, RenameTablesOptions options = null)
            => Provider.RenameTablesWithOptions(ast, mapping, options);

        public static OpenLineageColumnLineageResult OpenLineageColumnLineage(string sql, OpenLineageOptions options = null)
            => Provider.OpenLineageColumnLineage(sql, options);

        public static OpenLineageEventResult OpenLineageJobEvent(string sql, OpenLineageOptions options = null)
            => Provider.OpenLineageJobEvent(sql, options);

        public static OpenLineageEventResult OpenLineageRunEvent(string sql, OpenLineageOptions options = null)
            => Provider.OpenLineageRunEvent(sql, options);

        public static Expression[] AnnotateTypes(string sql, Dialect dialect = Dialect.Generic, ValidationSchema schema = null)
            => Provider.AnnotateTypes(sql, dialect, schema);

        public static string[] Generate(Expression[] ast, Dialect dialect = Dialect.Generic)
            => Provider.Generate(ast, dialect);

        public static QueryAnalysis AnalyzeQuery(string sql, AnalyzeQueryOptions options = null)
            => Provider.AnalyzeQuery(sql, options);

        public static ValidationResult Validate(string sql, Dialect dialect = Dialect.Generic)
            => Provider.Validate(sql, dialect);

        public static Expression[] ParseWithOptions(string sql, Dialect dialect = Dialect.Generic, ParseOptions options = null)
            => Provider.ParseWithOptions(sql, dialect, options);

        public static Expression ParseOneWithOptions(string sql, Dialect dialect = Dialect.Generic, ParseOptions options = null)
            => Provider.ParseOneWithOptions(sql, dialect, options);

        public static DataType ParseDataTypeWithOptions(string sql, Dialect dialect = Dialect.Generic, ParseOptions options = null)
            => Provider.ParseDataTypeWithOptions(sql, dialect, options);

        public static ValidationResult ValidateWithOptions(string sql, Dialect dialect = Dialect.Generic, ValidationOptions options = null)
            => Provider.ValidateWithOptions(sql, dialect, options);

        public static ValidationResult ValidateWithSchema(string sql, ValidationSchema schema, Dialect dialect = Dialect.Generic, SchemaValidationOptions options = null)
            => Provider.ValidateWithSchema(sql, schema, dialect, options);

        public static LineageNode LineageAt(int ordinal, string sql, Dialect dialect = Dialect.Generic)
            => Provider.LineageAt(ordinal, sql, dialect);

        public static LineageNode LineageAtWithSchema(int ordinal, string sql, ValidationSchema schema, Dialect dialect = Dialect.Generic)
            => Provider.LineageAtWithSchema(ordinal, sql, schema, dialect);

        public static QueryOutput OutputColumns(string sql, Dialect dialect = Dialect.Generic)
            => Provider.OutputColumns(sql, dialect);

        public static QueryOutput OutputColumnsWithSchema(string sql, ValidationSchema schema, Dialect dialect = Dialect.Generic)
            => Provider.OutputColumnsWithSchema(sql, schema, dialect);

        public static Expression[] SetLimit(Expression[] ast, ulong limit)
            => Provider.SetLimit(ast, limit);

        public static Expression[] SetOffset(Expression[] ast, ulong offset)
            => Provider.SetOffset(ast, offset);

        public static Expression[] SetOrderBy(Expression[] ast, Expression[] orderBy)
            => Provider.SetOrderBy(ast, orderBy);

        public static Expression BuildAst(BuilderPlan plan, Dialect readDialect = Dialect.Generic)
            => Provider.BuildAst(plan, readDialect);

        public static string BuildSql(BuilderPlan plan, Dialect readDialect = Dialect.Generic, Dialect outputDialect = Dialect.Generic)
            => Provider.BuildSql(plan, readDialect, outputDialect);

        public static VersionInfo VersionInfo
        {
            get
            {
                var assembly = typeof(Polyglot).Assembly;
                string wrapperVersion =
                    assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
                    ?? assembly.GetName().Version?.ToString()
                    ?? "0.0.0";

                return new VersionInfo
                {
                    NativeRuntimeVersion = Provider.Version(),
                    WrapperVersion = wrapperVersion,
                };
            }
        }

        // to be moved to static extensions with c# 14+:

        public static string TranspileDataType(string sql, Dialect fromDialect, Dialect toDialect)
        {
            DataType fromType = ParseDataType(sql, fromDialect);
            if (fromType != null)
                return GenerateDataType(fromType, toDialect);

            // like in regular Transpile - if no transpile result then return input
            return sql;
        }

        public static string GenerateOne(Expression ast, Dialect dialect = Dialect.Generic)
        {
            return Generate(new[] { ast }, dialect)?.FirstOrDefault();
        }
    }
}
