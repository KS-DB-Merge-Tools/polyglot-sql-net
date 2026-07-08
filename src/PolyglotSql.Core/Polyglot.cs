using System;

using PolyglotSql.Models;

namespace PolyglotSql
{
    public interface INativePolyglot
    {
        Token[] Tokenize(string sql, Dialect dialect = Dialect.Generic);
        string[] Transpile(string sql, Dialect fromDialect, Dialect toDialect);
        string[] TranspileWithOptions(string sql, Dialect fromDialect, Dialect toDialect, TranspileOptions options);
        string[] Format(string sql, Dialect dialect);
        string[] FormatWithOptions(string sql, Dialect dialect, FormatGuardOptions options);
        Expression[] Parse(string sql, Dialect dialect = Dialect.Generic);
        Expression ParseOne(string sql, Dialect dialect = Dialect.Generic);
        DiffResult Diff(string sql1, string sql2, Dialect dialect = Dialect.Generic);
        DataType ParseDataType(string sql, Dialect dialect = Dialect.Generic);
        string GenerateDataType(DataType dataType, Dialect dialect = Dialect.Generic);
    }

    public static class Polyglot
    {
        private static INativePolyglot? _provider;
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

        public static INativePolyglot Provider
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
