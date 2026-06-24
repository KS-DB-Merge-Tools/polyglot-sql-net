using System;
using System.Runtime.InteropServices;

namespace PolyglotSql
{
    public static class Polyglot
    {
        public static string Tokenize(string sql, Dialect dialect = Dialect.Generic)
        {
            IntPtr sqlPtr = IntPtr.Zero;
            IntPtr dialectPtr = IntPtr.Zero;

            try
            {
                sqlPtr = Marshal.StringToHGlobalAnsi(sql);
                dialectPtr = Marshal.StringToHGlobalAnsi(dialect.ToString().ToLowerInvariant());

                PolyglotNative.PolyglotResult result = PolyglotNative.polyglot_tokenize(sqlPtr, dialectPtr);

                if (result.Status != 0)
                {
                    string error = result.Error != IntPtr.Zero
                        ? Marshal.PtrToStringAnsi(result.Error)
                        : "Unknown error";

                    if (result.Error != IntPtr.Zero)
                        PolyglotNative.polyglot_free_string(result.Error);

                    throw new InvalidOperationException($"Polyglot tokenize error (status {result.Status}): {error}");
                }

                string json = result.Data != IntPtr.Zero
                    ? Marshal.PtrToStringAnsi(result.Data)
                    : "[]";

                PolyglotNative.polyglot_free_result(result);

                return json;
            }
            finally
            {
                if (sqlPtr != IntPtr.Zero)
                    Marshal.FreeHGlobal(sqlPtr);
                if (dialectPtr != IntPtr.Zero)
                    Marshal.FreeHGlobal(dialectPtr);
            }
        }

        public static string Transpile(string sql, Dialect fromDialect, Dialect toDialect)
        {
            IntPtr sqlPtr = IntPtr.Zero;
            IntPtr fromDialectPtr = IntPtr.Zero;
            IntPtr toDialectPtr = IntPtr.Zero;

            try
            {
                sqlPtr = Marshal.StringToHGlobalAnsi(sql);
                fromDialectPtr = Marshal.StringToHGlobalAnsi(fromDialect.ToString().ToLowerInvariant());
                toDialectPtr = Marshal.StringToHGlobalAnsi(toDialect.ToString().ToLowerInvariant());

                PolyglotNative.PolyglotResult result = PolyglotNative.polyglot_transpile(sqlPtr, fromDialectPtr, toDialectPtr);

                if (result.Status != 0)
                {
                    string error = result.Error != IntPtr.Zero
                        ? Marshal.PtrToStringAnsi(result.Error)
                        : "Unknown error";

                    if (result.Error != IntPtr.Zero)
                        PolyglotNative.polyglot_free_string(result.Error);

                    throw new InvalidOperationException($"Polyglot transpile error (status {result.Status}): {error}");
                }

                string json = result.Data != IntPtr.Zero
                    ? Marshal.PtrToStringAnsi(result.Data)
                    : "[]";

                PolyglotNative.polyglot_free_result(result);

                return json;
            }
            finally
            {
                if (sqlPtr != IntPtr.Zero)
                    Marshal.FreeHGlobal(sqlPtr);
                if (fromDialectPtr != IntPtr.Zero)
                    Marshal.FreeHGlobal(fromDialectPtr);
                if (toDialectPtr != IntPtr.Zero)
                    Marshal.FreeHGlobal(toDialectPtr);
            }
        }

        public static string TranspileWithOptions(string sql, Dialect fromDialect, Dialect toDialect, string optionsJson)
        {
            IntPtr sqlPtr = IntPtr.Zero;
            IntPtr fromDialectPtr = IntPtr.Zero;
            IntPtr toDialectPtr = IntPtr.Zero;
            IntPtr optionsJsonPtr = IntPtr.Zero;

            try
            {
                sqlPtr = Marshal.StringToHGlobalAnsi(sql);
                fromDialectPtr = Marshal.StringToHGlobalAnsi(fromDialect.ToString().ToLowerInvariant());
                toDialectPtr = Marshal.StringToHGlobalAnsi(toDialect.ToString().ToLowerInvariant());
                optionsJsonPtr = Marshal.StringToHGlobalAnsi(optionsJson);

                PolyglotNative.PolyglotResult result = PolyglotNative.polyglot_transpile_with_options(sqlPtr, fromDialectPtr, toDialectPtr, optionsJsonPtr);

                if (result.Status != 0)
                {
                    string error = result.Error != IntPtr.Zero
                        ? Marshal.PtrToStringAnsi(result.Error)
                        : "Unknown error";

                    if (result.Error != IntPtr.Zero)
                        PolyglotNative.polyglot_free_string(result.Error);

                    throw new InvalidOperationException($"Polyglot transpile error (status {result.Status}): {error}");
                }

                string json = result.Data != IntPtr.Zero
                    ? Marshal.PtrToStringAnsi(result.Data)
                    : "[]";

                PolyglotNative.polyglot_free_result(result);

                return json;
            }
            finally
            {
                if (sqlPtr != IntPtr.Zero)
                    Marshal.FreeHGlobal(sqlPtr);
                if (fromDialectPtr != IntPtr.Zero)
                    Marshal.FreeHGlobal(fromDialectPtr);
                if (toDialectPtr != IntPtr.Zero)
                    Marshal.FreeHGlobal(toDialectPtr);
                if (optionsJsonPtr != IntPtr.Zero)
                    Marshal.FreeHGlobal(optionsJsonPtr);
            }
        }

        public static string Parse(string sql, Dialect dialect = Dialect.Generic)
        {
            IntPtr sqlPtr = IntPtr.Zero;
            IntPtr dialectPtr = IntPtr.Zero;

            try
            {
                sqlPtr = Marshal.StringToHGlobalAnsi(sql);
                dialectPtr = Marshal.StringToHGlobalAnsi(dialect.ToString().ToLowerInvariant());

                PolyglotNative.PolyglotResult result = PolyglotNative.polyglot_parse(sqlPtr, dialectPtr);

                if (result.Status != 0)
                {
                    string error = result.Error != IntPtr.Zero
                        ? Marshal.PtrToStringAnsi(result.Error)
                        : "Unknown error";

                    if (result.Error != IntPtr.Zero)
                        PolyglotNative.polyglot_free_string(result.Error);

                    throw new InvalidOperationException($"Polyglot parse error (status {result.Status}): {error}");
                }

                string json = result.Data != IntPtr.Zero
                    ? Marshal.PtrToStringAnsi(result.Data)
                    : "[]";

                PolyglotNative.polyglot_free_result(result);

                return json;
            }
            finally
            {
                if (sqlPtr != IntPtr.Zero)
                    Marshal.FreeHGlobal(sqlPtr);
                if (dialectPtr != IntPtr.Zero)
                    Marshal.FreeHGlobal(dialectPtr);
            }
        }

        public static string ParseOne(string sql, Dialect dialect = Dialect.Generic)
        {
            IntPtr sqlPtr = IntPtr.Zero;
            IntPtr dialectPtr = IntPtr.Zero;

            try
            {
                sqlPtr = Marshal.StringToHGlobalAnsi(sql);
                dialectPtr = Marshal.StringToHGlobalAnsi(dialect.ToString().ToLowerInvariant());

                PolyglotNative.PolyglotResult result = PolyglotNative.polyglot_parse_one(sqlPtr, dialectPtr);

                if (result.Status != 0)
                {
                    string error = result.Error != IntPtr.Zero
                        ? Marshal.PtrToStringAnsi(result.Error)
                        : "Unknown error";

                    if (result.Error != IntPtr.Zero)
                        PolyglotNative.polyglot_free_string(result.Error);

                    throw new InvalidOperationException($"Polyglot parse_one error (status {result.Status}): {error}");
                }

                string json = result.Data != IntPtr.Zero
                    ? Marshal.PtrToStringAnsi(result.Data)
                    : "[]";

                PolyglotNative.polyglot_free_result(result);

                return json;
            }
            finally
            {
                if (sqlPtr != IntPtr.Zero)
                    Marshal.FreeHGlobal(sqlPtr);
                if (dialectPtr != IntPtr.Zero)
                    Marshal.FreeHGlobal(dialectPtr);
            }
        }

        public static string Diff(string sql1, string sql2, Dialect dialect = Dialect.Generic)
        {
            IntPtr sql1Ptr = IntPtr.Zero;
            IntPtr sql2Ptr = IntPtr.Zero;
            IntPtr dialectPtr = IntPtr.Zero;

            try
            {
                sql1Ptr = Marshal.StringToHGlobalAnsi(sql1);
                sql2Ptr = Marshal.StringToHGlobalAnsi(sql2);
                dialectPtr = Marshal.StringToHGlobalAnsi(dialect.ToString().ToLowerInvariant());

                PolyglotNative.PolyglotResult result = PolyglotNative.polyglot_diff(sql1Ptr, sql2Ptr, dialectPtr);

                if (result.Status != 0)
                {
                    string error = result.Error != IntPtr.Zero
                        ? Marshal.PtrToStringAnsi(result.Error)
                        : "Unknown error";

                    if (result.Error != IntPtr.Zero)
                        PolyglotNative.polyglot_free_string(result.Error);

                    throw new InvalidOperationException($"Polyglot diff error (status {result.Status}): {error}");
                }

                string json = result.Data != IntPtr.Zero
                    ? Marshal.PtrToStringAnsi(result.Data)
                    : "[]";

                PolyglotNative.polyglot_free_result(result);

                return json;
            }
            finally
            {
                if (sql1Ptr != IntPtr.Zero)
                    Marshal.FreeHGlobal(sql1Ptr);
                if (sql2Ptr != IntPtr.Zero)
                    Marshal.FreeHGlobal(sql2Ptr);
                if (dialectPtr != IntPtr.Zero)
                    Marshal.FreeHGlobal(dialectPtr);
            }
        }
    }
}
