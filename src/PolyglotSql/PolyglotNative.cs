using System;
using System.Runtime.InteropServices;

namespace PolyglotSql
{
    public static class PolyglotNative
    {
        private const string LibName = "polyglot_sql_ffi";

        [StructLayout(LayoutKind.Sequential)]
        public struct PolyglotResult
        {
            public IntPtr Data;
            public IntPtr Error;
            public int Status;
        }

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern PolyglotResult polyglot_tokenize(IntPtr sql, IntPtr dialect);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern PolyglotResult polyglot_transpile(IntPtr sql, IntPtr fromDialect, IntPtr toDialect);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern PolyglotResult polyglot_transpile_with_options(IntPtr sql, IntPtr fromDialect, IntPtr toDialect, IntPtr optionsJson);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern PolyglotResult polyglot_parse(IntPtr sql, IntPtr dialect);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern PolyglotResult polyglot_parse_one(IntPtr sql, IntPtr dialect);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern PolyglotResult polyglot_diff(IntPtr sql1, IntPtr sql2, IntPtr dialect);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern void polyglot_free_string(IntPtr s);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void polyglot_free_result(PolyglotResult result);

        public static string Tokenize(string sql, string dialect = "generic")
        {
            IntPtr sqlPtr = IntPtr.Zero;
            IntPtr dialectPtr = IntPtr.Zero;

            try
            {
                sqlPtr = Marshal.StringToHGlobalAnsi(sql);
                dialectPtr = Marshal.StringToHGlobalAnsi(dialect);

                PolyglotResult result = polyglot_tokenize(sqlPtr, dialectPtr);

                if (result.Status != 0)
                {
                    string error = result.Error != IntPtr.Zero
                        ? Marshal.PtrToStringAnsi(result.Error)
                        : "Unknown error";

                    if (result.Error != IntPtr.Zero)
                        polyglot_free_string(result.Error);

                    throw new InvalidOperationException($"Polyglot tokenize error (status {result.Status}): {error}");
                }

                string json = result.Data != IntPtr.Zero
                    ? Marshal.PtrToStringAnsi(result.Data)
                    : "[]";

                polyglot_free_result(result);

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

        public static string Transpile(string sql, string fromDialect, string toDialect)
        {
            IntPtr sqlPtr = IntPtr.Zero;
            IntPtr fromDialectPtr = IntPtr.Zero;
            IntPtr toDialectPtr = IntPtr.Zero;

            try
            {
                sqlPtr = Marshal.StringToHGlobalAnsi(sql);
                fromDialectPtr = Marshal.StringToHGlobalAnsi(fromDialect);
                toDialectPtr = Marshal.StringToHGlobalAnsi(toDialect);

                PolyglotResult result = polyglot_transpile(sqlPtr, fromDialectPtr, toDialectPtr);

                if (result.Status != 0)
                {
                    string error = result.Error != IntPtr.Zero
                        ? Marshal.PtrToStringAnsi(result.Error)
                        : "Unknown error";

                    if (result.Error != IntPtr.Zero)
                        polyglot_free_string(result.Error);

                    throw new InvalidOperationException($"Polyglot transpile error (status {result.Status}): {error}");
                }

                string json = result.Data != IntPtr.Zero
                    ? Marshal.PtrToStringAnsi(result.Data)
                    : "[]";

                polyglot_free_result(result);

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

        public static string TranspileWithOptions(string sql, string fromDialect, string toDialect, string optionsJson)
        {
            IntPtr sqlPtr = IntPtr.Zero;
            IntPtr fromDialectPtr = IntPtr.Zero;
            IntPtr toDialectPtr = IntPtr.Zero;
            IntPtr optionsJsonPtr = IntPtr.Zero;

            try
            {
                sqlPtr = Marshal.StringToHGlobalAnsi(sql);
                fromDialectPtr = Marshal.StringToHGlobalAnsi(fromDialect);
                toDialectPtr = Marshal.StringToHGlobalAnsi(toDialect);
                optionsJsonPtr = Marshal.StringToHGlobalAnsi(optionsJson);

                PolyglotResult result = polyglot_transpile_with_options(sqlPtr, fromDialectPtr, toDialectPtr, optionsJsonPtr);

                if (result.Status != 0)
                {
                    string error = result.Error != IntPtr.Zero
                        ? Marshal.PtrToStringAnsi(result.Error)
                        : "Unknown error";

                    if (result.Error != IntPtr.Zero)
                        polyglot_free_string(result.Error);

                    throw new InvalidOperationException($"Polyglot transpile error (status {result.Status}): {error}");
                }

                string json = result.Data != IntPtr.Zero
                    ? Marshal.PtrToStringAnsi(result.Data)
                    : "[]";

                polyglot_free_result(result);

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

        public static string Parse(string sql, string dialect = "generic")
        {
            IntPtr sqlPtr = IntPtr.Zero;
            IntPtr dialectPtr = IntPtr.Zero;

            try
            {
                sqlPtr = Marshal.StringToHGlobalAnsi(sql);
                dialectPtr = Marshal.StringToHGlobalAnsi(dialect);

                PolyglotResult result = polyglot_parse(sqlPtr, dialectPtr);

                if (result.Status != 0)
                {
                    string error = result.Error != IntPtr.Zero
                        ? Marshal.PtrToStringAnsi(result.Error)
                        : "Unknown error";

                    if (result.Error != IntPtr.Zero)
                        polyglot_free_string(result.Error);

                    throw new InvalidOperationException($"Polyglot parse error (status {result.Status}): {error}");
                }

                string json = result.Data != IntPtr.Zero
                    ? Marshal.PtrToStringAnsi(result.Data)
                    : "[]";

                polyglot_free_result(result);

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

        public static string ParseOne(string sql, string dialect = "generic")
        {
            IntPtr sqlPtr = IntPtr.Zero;
            IntPtr dialectPtr = IntPtr.Zero;

            try
            {
                sqlPtr = Marshal.StringToHGlobalAnsi(sql);
                dialectPtr = Marshal.StringToHGlobalAnsi(dialect);

                PolyglotResult result = polyglot_parse_one(sqlPtr, dialectPtr);

                if (result.Status != 0)
                {
                    string error = result.Error != IntPtr.Zero
                        ? Marshal.PtrToStringAnsi(result.Error)
                        : "Unknown error";

                    if (result.Error != IntPtr.Zero)
                        polyglot_free_string(result.Error);

                    throw new InvalidOperationException($"Polyglot parse_one error (status {result.Status}): {error}");
                }

                string json = result.Data != IntPtr.Zero
                    ? Marshal.PtrToStringAnsi(result.Data)
                    : "[]";

                polyglot_free_result(result);

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

        public static string Diff(string sql1, string sql2, string dialect = "generic")
        {
            IntPtr sql1Ptr = IntPtr.Zero;
            IntPtr sql2Ptr = IntPtr.Zero;
            IntPtr dialectPtr = IntPtr.Zero;

            try
            {
                sql1Ptr = Marshal.StringToHGlobalAnsi(sql1);
                sql2Ptr = Marshal.StringToHGlobalAnsi(sql2);
                dialectPtr = Marshal.StringToHGlobalAnsi(dialect);

                PolyglotResult result = polyglot_diff(sql1Ptr, sql2Ptr, dialectPtr);

                if (result.Status != 0)
                {
                    string error = result.Error != IntPtr.Zero
                        ? Marshal.PtrToStringAnsi(result.Error)
                        : "Unknown error";

                    if (result.Error != IntPtr.Zero)
                        polyglot_free_string(result.Error);

                    throw new InvalidOperationException($"Polyglot diff error (status {result.Status}): {error}");
                }

                string json = result.Data != IntPtr.Zero
                    ? Marshal.PtrToStringAnsi(result.Data)
                    : "[]";

                polyglot_free_result(result);

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
