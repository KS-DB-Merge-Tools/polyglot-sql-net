using System;
using System.Collections.Generic;
using System.Reflection;

using PolyglotSql.Models;

namespace PolyglotSql
{
    public static class Polyglot
    {
        internal static INativePolyglot? _provider; // internal to test dialect list
        private static readonly object _lock = new object();

        internal static void RegisterProvider(INativePolyglot provider)
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

        public static string TranspileDataType(string sql, Dialect fromDialect, Dialect toDialect)
        {
            DataType fromType = ParseDataType(sql, fromDialect);
            if (fromType != null)
                return GenerateDataType(fromType, toDialect);

            // like in regular Transpile - if no transpile result then return input
            return sql;
        }
    }
}
