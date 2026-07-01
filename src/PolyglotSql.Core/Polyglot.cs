using System;
using System.Runtime.InteropServices;

namespace PolyglotSql
{
    public interface INativePolyglot
    {
        string Tokenize(string sql, Dialect dialect = Dialect.Generic);
        string[] Transpile(string sql, Dialect fromDialect, Dialect toDialect);
        string[] TranspileWithOptions(string sql, Dialect fromDialect, Dialect toDialect, string optionsJson);
        string Parse(string sql, Dialect dialect = Dialect.Generic);
        string ParseOne(string sql, Dialect dialect = Dialect.Generic);
        string Diff(string sql1, string sql2, Dialect dialect = Dialect.Generic);
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

        public static string Tokenize(string sql, Dialect dialect = Dialect.Generic)
            => Provider.Tokenize(sql, dialect);

        public static string[] Transpile(string sql, Dialect fromDialect, Dialect toDialect)
            => Provider.Transpile(sql, fromDialect, toDialect);

        public static string[] TranspileWithOptions(string sql, Dialect fromDialect, Dialect toDialect, string optionsJson)
            => Provider.TranspileWithOptions(sql, fromDialect, toDialect, optionsJson);

        public static string Parse(string sql, Dialect dialect = Dialect.Generic)
            => Provider.Parse(sql, dialect);

        public static string ParseOne(string sql, Dialect dialect = Dialect.Generic)
            => Provider.ParseOne(sql, dialect);

        public static string Diff(string sql1, string sql2, Dialect dialect = Dialect.Generic)
            => Provider.Diff(sql1, sql2, dialect);

        public static DataType ParseDataType(string sql, Dialect dialect = Dialect.Generic)
            => Provider.ParseDataType(sql, dialect);

        public static string GenerateDataType(DataType dataType, Dialect dialect = Dialect.Generic)
            => Provider.GenerateDataType(dataType, dialect);
    }
}
