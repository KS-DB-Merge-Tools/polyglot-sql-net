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
    }
}
